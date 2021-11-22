using Cerera.Services;
using UnityEngine.UI;

public class ResourcePanel : HubMenu, IDependentObject
{
    private Text[] _resourcesVolume;

    private Company _company;

    [Inject]
    private void Construct(Company company)
    {
        _company = company;
    }

    private void Awake()
    {
        _resourcesVolume = new Text[4];

        for (int i = 0; i < _resourcesVolume.Length; i++)
        {
            _resourcesVolume[i] = transform.GetChild(i).GetChild(0).GetComponent<Text>();
        }
        UpdatePanel();
    }

    public void UpdatePanel()
    {
        for (int i = 0; i < _resourcesVolume.Length ; i++)
        {
            _resourcesVolume[i].text = _company.GetResourceAmount((GAME_RESOURCE)i).ToString();
        }
    }

    public override void OnEvent(HUB_EVENT_TYPE eventType, object[] data)
    {
        UpdatePanel();
    }
}
