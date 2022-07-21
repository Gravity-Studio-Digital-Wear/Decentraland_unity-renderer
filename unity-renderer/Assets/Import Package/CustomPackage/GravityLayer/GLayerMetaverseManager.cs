using UnityEngine;
using GravityLayer;
using GravityLayer.Wearables;

public class GLayerMetaverseManager : MonoBehaviour
{
    
    public MetaverseEntryPoint GLMetaverseEntryPoint;
    public string Account = "0x39b7d171e693b3e6270c40899e46c90016e3bd71";

    [SerializeField] protected string _apiUrl = "https://gravity-dev.easychain.dev/api";
    [SerializeField] protected string _secret = "4Hf573CnutWhOA3b4yo1H5Sh";

    void Awake()
    {
        GLMetaverseEntryPoint = new MetaverseEntryPoint(_apiUrl, _secret);
        PlayerPrefs.SetString("Account", Account);
        ConsoleTest();
    }

    void ConsoleTest()
    {
        GetAllInteroperableWearables();

        GetUserInteroperableWearables();

        // The coat seemed familiar yet summoned the appearance of some strange moon-creature shimmering in half-light.
        // https://testnets.opensea.io/assets/mumbai/0x2953399124f0cbb46d2cbacd8a89cf0599974963/75437324160650951662245703982020702172073797313123328702383515790577235918948
        GetNFTModel("0x2953399124f0cbb46d2cbacd8a89cf0599974963", "75437324160650951662245703982020702172073797313123328702383515790577235918948");
    }

    async void GetAllInteroperableWearables()
    {
        await GLMetaverseEntryPoint.Stock.FetchAllInteroperableWearables();
        Debug.Log("Products in stock " + GLMetaverseEntryPoint.Stock.Wearables.Count);
    }

    async void GetUserInteroperableWearables()
    {
        await GLMetaverseEntryPoint.Wardrobe.FetchInteroperableWearables(Account);
        Debug.Log("User's products count " + GLMetaverseEntryPoint.Wardrobe.Wearables.Count);
    }

    async void GetNFTModel(string contractAddress, string tokenId)
    {
        WearableWithMetadata wearable = new WearableWithMetadata("Fluted Coat");
        wearable.Metadata = await GLMetaverseEntryPoint.WearableServices.GetWearableMetadata(contractAddress, tokenId);
        Debug.Log(wearable.Title + "\n" + wearable.Metadata[0].ModelUrl);
        UnityEngine.UI.Image img = FindObjectOfType<UnityEngine.UI.Image>();
        foreach (var attr in wearable.Metadata[0].Attributes)
        {
            Debug.Log("  " + attr.Key + ": " + attr.Value + "\n");
            if(img!= null)
                img.sprite = Sprite.Create(wearable.Metadata[0].PreviewImage, new Rect(0,0, wearable.Metadata[0].PreviewImage.width,wearable.Metadata[0].PreviewImage.height),Vector2.zero );
        }
    }
}
