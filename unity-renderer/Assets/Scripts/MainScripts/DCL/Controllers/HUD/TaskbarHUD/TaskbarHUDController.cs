using System;
using System.Linq;
using DCL;
using DCL.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class TaskbarHUDController : IHUD
{
    [Serializable]
    public struct Configuration
    {
        public bool enableVoiceChat;
        public bool enableQuestPanel;
    }

    public TaskbarHUDView view;
    public WorldChatWindowController worldChatWindowHud;
    public PrivateChatWindowController privateChatWindow;
    private PublicChatChannelController publicChatChannel;
    public FriendsHUDController friendsHud;

    IMouseCatcher mouseCatcher;
    protected IChatController chatController;
    protected IFriendsController friendsController;

    private InputAction_Trigger toggleFriendsTrigger;
    private InputAction_Trigger closeWindowTrigger;
    private InputAction_Trigger toggleWorldChatTrigger;
    private Transform experiencesViewerTransform;

    public event Action OnAnyTaskbarButtonClicked;

    public RectTransform socialTooltipReference { get => view.socialTooltipReference; }

    internal BaseVariable<Transform> isExperiencesViewerInitialized => DataStore.i.experiencesViewer.isInitialized;
    internal BaseVariable<bool> isExperiencesViewerOpen => DataStore.i.experiencesViewer.isOpen;
    internal BaseVariable<int> numOfLoadedExperiences => DataStore.i.experiencesViewer.numOfLoadedExperiences;

    protected internal virtual TaskbarHUDView CreateView() { return TaskbarHUDView.Create(this, chatController, friendsController); }

    public void Initialize(
        IMouseCatcher mouseCatcher,
        IChatController chatController,
        IFriendsController friendsController)
    {
        this.friendsController = friendsController;
        this.mouseCatcher = mouseCatcher;
        this.chatController = chatController;

        view = CreateView();

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
            mouseCatcher.OnMouseLock += MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock += MouseCatcher_OnMouseUnlock;
        }

        view.chatHeadsGroup.OnHeadToggleOn += ChatHeadsGroup_OnHeadOpen;
        view.chatHeadsGroup.OnHeadToggleOff += ChatHeadsGroup_OnHeadClose;

        view.leftWindowContainerLayout.enabled = false;

        view.OnChatToggleOff += View_OnChatToggleOff;
        view.OnChatToggleOn += View_OnChatToggleOn;
        view.OnFriendsToggleOff += View_OnFriendsToggleOff;
        view.OnFriendsToggleOn += View_OnFriendsToggleOn;
        view.OnExperiencesToggleOff += View_OnExperiencesToggleOff;
        view.OnExperiencesToggleOn += View_OnExperiencesToggleOn;

        toggleFriendsTrigger = Resources.Load<InputAction_Trigger>("ToggleFriends");
        toggleFriendsTrigger.OnTriggered -= ToggleFriendsTrigger_OnTriggered;
        toggleFriendsTrigger.OnTriggered += ToggleFriendsTrigger_OnTriggered;

        closeWindowTrigger = Resources.Load<InputAction_Trigger>("CloseWindow");
        closeWindowTrigger.OnTriggered -= CloseWindowTrigger_OnTriggered;
        closeWindowTrigger.OnTriggered += CloseWindowTrigger_OnTriggered;

        toggleWorldChatTrigger = Resources.Load<InputAction_Trigger>("ToggleWorldChat");
        toggleWorldChatTrigger.OnTriggered -= ToggleWorldChatTrigger_OnTriggered;
        toggleWorldChatTrigger.OnTriggered += ToggleWorldChatTrigger_OnTriggered;

        isExperiencesViewerOpen.OnChange += IsExperiencesViewerOpenChanged;

        view.leftWindowContainerAnimator.Show();

        CommonScriptableObjects.isTaskbarHUDInitialized.Set(true);
        DataStore.i.builderInWorld.showTaskBar.OnChange += SetVisibility;

        isExperiencesViewerInitialized.OnChange += InitializeExperiencesViewer;
        InitializeExperiencesViewer(isExperiencesViewerInitialized.Get(), null);

        numOfLoadedExperiences.OnChange += NumOfLoadedExperiencesChanged;
        NumOfLoadedExperiencesChanged(numOfLoadedExperiences.Get(), 0);
    }

    private void ChatHeadsGroup_OnHeadClose(TaskbarButton obj) { privateChatWindow.SetVisibility(false); }

    private void View_OnFriendsToggleOn()
    {
        friendsHud?.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnFriendsToggleOff() { friendsHud?.SetVisibility(false); }

    private void View_OnExperiencesToggleOn()
    {
        isExperiencesViewerOpen.Set(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnExperiencesToggleOff() { isExperiencesViewerOpen.Set(false); }

    private void ToggleFriendsTrigger_OnTriggered(DCLAction_Trigger action)
    {
        if (!view.friendsButton.transform.parent.gameObject.activeSelf)
            return;

        OnFriendsToggleInputPress();
    }

    private void ToggleWorldChatTrigger_OnTriggered(DCLAction_Trigger action) { OnWorldChatToggleInputPress(); }

    private void CloseWindowTrigger_OnTriggered(DCLAction_Trigger action) { OnCloseWindowToggleInputPress(); }

    private void View_OnChatToggleOn()
    {
        worldChatWindowHud.SetVisibility(true);
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    private void View_OnChatToggleOff()
    {
        worldChatWindowHud.SetVisibility(false);
    }

    private void ChatHeadsGroup_OnHeadOpen(TaskbarButton taskbarBtn)
    {
        ChatHeadButton head = taskbarBtn as ChatHeadButton;

        if (taskbarBtn == null)
            return;

        OpenPrivateChatWindow(head.profile.userId);
    }

    private void MouseCatcher_OnMouseUnlock() { view.leftWindowContainerAnimator.Show(); }

    private void MouseCatcher_OnMouseLock()
    {
        view.leftWindowContainerAnimator.Hide();

        foreach (var btn in view.GetButtonList())
        {
            btn.SetToggleState(false);
        }
    }

    public void AddWorldChatWindow(WorldChatWindowController controller)
    {
        if (controller == null || controller.View == null)
        {
            Debug.LogWarning("AddChatWindow >>> World Chat Window doesn't exist yet!");
            return;
        }

        if (controller.View.Transform.parent == view.leftWindowContainer)
            return;

        controller.View.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        worldChatWindowHud = controller;

        view.OnAddChatWindow();
        worldChatWindowHud.View.OnClose += () => { view.friendsButton.SetToggleState(false, false); };
        view.chatButton.SetToggleState(false);
    }

    public void OpenFriendsWindow() { view.friendsButton.SetToggleState(true); }

    public void OpenPrivateChatTo(string userId)
    {
        var button = view.chatHeadsGroup.AddChatHead(userId, (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        button.toggleButton.onClick.Invoke();
        privateChatWindow.Configure(userId);
        worldChatWindowHud.SetVisibility(false);
        privateChatWindow.SetVisibility(true);
    }

    public void OpenPublicChatChannel(string channelId)
    {
        worldChatWindowHud.SetVisibility(false);
        publicChatChannel.Setup(channelId);
        publicChatChannel.SetVisibility(true);
    }
    
    public void OpenChatList()
    {
        worldChatWindowHud.SetVisibility(true);
    }

    public void AddPrivateChatWindow(PrivateChatWindowController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddPrivateChatWindow >>> Private Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.Transform.parent == view.leftWindowContainer)
            return;

        controller.view.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        privateChatWindow = controller;

        privateChatWindow.view.OnMinimize += () =>
        {
            ChatHeadButton btn = view.GetButtonList()
                                     .FirstOrDefault(
                                         (x) => x is ChatHeadButton &&
                                                (x as ChatHeadButton).profile.userId == privateChatWindow.conversationUserId) as
                ChatHeadButton;

            if (btn != null)
                btn.SetToggleState(false, false);
        };

        privateChatWindow.view.OnClose += () =>
        {
            ChatHeadButton btn = view.GetButtonList()
                                     .FirstOrDefault(
                                         (x) => x is ChatHeadButton &&
                                                (x as ChatHeadButton).profile.userId == privateChatWindow.conversationUserId) as
                ChatHeadButton;

            if (btn != null)
            {
                btn.SetToggleState(false, false);
                view.chatHeadsGroup.RemoveChatHead(btn);
            }
        };
    }

    public void AddPublicChatChannel(PublicChatChannelController controller)
    {
        if (controller?.view == null)
        {
            Debug.LogWarning("AddPublicChatChannel >>> Public Chat Window doesn't exist yet!");
            return;
        }

        if (controller.view.Transform.parent == view.leftWindowContainer) return;

        controller.view.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();
        
        publicChatChannel = controller;
    }

    public void AddFriendsWindow(FriendsHUDController controller)
    {
        if (controller == null || controller.view == null)
        {
            Debug.LogWarning("AddFriendsWindow >>> Friends window doesn't exist yet!");
            return;
        }

        if (controller.view.Transform.parent == view.leftWindowContainer)
            return;

        controller.view.Transform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform?.SetAsLastSibling();

        friendsHud = controller;
        view.OnAddFriendsWindow();
        friendsHud.view.OnClose += () =>
        {
            view.friendsButton.SetToggleState(false, false);
        };

        friendsHud.view.OnDeleteConfirmation += (userIdToRemove) => { view.chatHeadsGroup.RemoveChatHead(userIdToRemove); };
    }

    internal void InitializeExperiencesViewer(Transform currentViewTransform, Transform previousViewTransform)
    {
        if (currentViewTransform == null)
            return;

        experiencesViewerTransform = currentViewTransform;
        experiencesViewerTransform.SetParent(view.leftWindowContainer, false);
        experiencesViewerTransform.SetAsLastSibling();

        view.OnAddExperiencesWindow();
    }

    private void IsExperiencesViewerOpenChanged(bool current, bool previous)
    {
        if (current)
            return;

        view.experiencesButton.SetToggleState(false, false);
    }

    private void NumOfLoadedExperiencesChanged(int current, int previous)
    {
        view.SetExperiencesVisbility(current > 0);

        if (current == 0)
            View_OnExperiencesToggleOff();
    }

    public void OnAddVoiceChat() { view.OnAddVoiceChat(); }

    public void DisableFriendsWindow()
    {
        view.friendsButton.transform.parent.gameObject.SetActive(false);
        view.chatHeadsGroup.ClearChatHeads();
    }

    private void OpenPrivateChatWindow(string userId)
    {
        privateChatWindow.Configure(userId);
        privateChatWindow.SetVisibility(true);
        privateChatWindow.ForceFocus();
        OnAnyTaskbarButtonClicked?.Invoke();
    }

    public void Dispose()
    {
        if (view != null)
        {
            view.chatHeadsGroup.OnHeadToggleOn -= ChatHeadsGroup_OnHeadOpen;
            view.chatHeadsGroup.OnHeadToggleOff -= ChatHeadsGroup_OnHeadClose;

            view.OnChatToggleOff -= View_OnChatToggleOff;
            view.OnChatToggleOn -= View_OnChatToggleOn;
            view.OnFriendsToggleOff -= View_OnFriendsToggleOff;
            view.OnFriendsToggleOn -= View_OnFriendsToggleOn;
            view.OnExperiencesToggleOff -= View_OnExperiencesToggleOff;
            view.OnExperiencesToggleOn -= View_OnExperiencesToggleOn;

            Object.Destroy(view.gameObject);
        }

        if (mouseCatcher != null)
        {
            mouseCatcher.OnMouseLock -= MouseCatcher_OnMouseLock;
            mouseCatcher.OnMouseUnlock -= MouseCatcher_OnMouseUnlock;
        }

        if (toggleFriendsTrigger != null)
            toggleFriendsTrigger.OnTriggered -= ToggleFriendsTrigger_OnTriggered;

        if (closeWindowTrigger != null)
            closeWindowTrigger.OnTriggered -= CloseWindowTrigger_OnTriggered;

        if (toggleWorldChatTrigger != null)
            toggleWorldChatTrigger.OnTriggered -= ToggleWorldChatTrigger_OnTriggered;

        DataStore.i.builderInWorld.showTaskBar.OnChange -= SetVisibility;
        isExperiencesViewerOpen.OnChange -= IsExperiencesViewerOpenChanged;
        isExperiencesViewerInitialized.OnChange -= InitializeExperiencesViewer;
        numOfLoadedExperiences.OnChange -= NumOfLoadedExperiencesChanged;
    }

    public void SetVisibility(bool visible, bool previus) { SetVisibility(visible); }

    public void SetVisibility(bool visible) { view.SetVisibility(visible); }

    public void OnWorldChatToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null;

        if (anyInputFieldIsSelected) return;
        
        friendsHud.SetVisibility(false);
        worldChatWindowHud.OpenLastActiveChat();
    }

    public void OnCloseWindowToggleInputPress()
    {
        if (mouseCatcher.isLocked)
            return;

        view.chatButton.SetToggleState(false, false);
        publicChatChannel.ResetInputField();
        publicChatChannel.view.ActivatePreview();
    }

    public void SetVoiceChatRecording(bool recording) { view?.voiceChatButton.SetOnRecording(recording); }

    public void SetVoiceChatEnabledByScene(bool enabled) { view?.voiceChatButton.SetEnabledByScene(enabled); }

    private void OnFriendsToggleInputPress()
    {
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>() != null &&
                                       (!worldChatWindowHud.IsInputFieldFocused || !worldChatWindowHud.IsPreview);

        if (anyInputFieldIsSelected)
            return;

        Utils.UnlockCursor();
        view.leftWindowContainerAnimator.Show();
        view.friendsButton.SetToggleState(!view.friendsButton.toggledOn);
    }
}