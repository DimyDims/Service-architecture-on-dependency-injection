using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Cerera.Services.Hub
{
    public sealed class Hub : ServicesConfigurator, IHub, IPointerDownHandler, IDependentObject
    {
        [SerializeField] private Tooltip _tooltip;
        [SerializeField] private Dialog _dialog;

        private readonly Dictionary<Type, HubMenu> _menus = new Dictionary<Type, HubMenu>();

        private OpenableHubMenu _openedMenu;

        private Localization _localization;

        [Inject]
        private void Construct(Localization localization)
        {
            _localization = localization;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddInstance<IHub>(this)
                .AddInstance(_tooltip)
                .AddInstance<IDialog>(_dialog)
                .AddSingleton<CharacterClassAbilitiesDatabase>(ResourcePath.Databases.CharacterClassAbilities)
                .AddSingleton<CharacterClassChanger>()
                .AddSingleton<CharacterAbilityBuyer>()
                .AddTransient<ShipAbilityPricingDatabase>(ResourcePath.Databases.ShipAbilitiesPrices)
                .AddSingleton<ShipAbilityBuyer>()
                .AddSingleton<StationItemBuyer>()
                .AddSingleton<CharacterEquipper>()
                .AddSingleton<ItemsSeller>()
                .AddSingleton<CharacterUpgrader>();
        }

        public override void OnServicesInitialized()
        {
            InitializeAllMenus();
        }

        private void Start()
        {
            CloseAllOpenableMenus();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            CloseOpenedMenu();
        }

        private void InitializeAllMenus()
        {
            foreach (HubMenu menu in FindObjectsOfType(typeof(HubMenu), true))
            {
                _menus.Add(menu.GetType(), menu);
            }

            foreach (var pair in _menus)
            {
                if (pair.Value is OpenableHubMenu)
                {
                    pair.Value.gameObject.SetActive(true);
                }
            }
        }

        private void CloseAllOpenableMenus()
        {
            foreach (var pair in _menus)
            {
                if (pair.Value is OpenableHubMenu)
                {
                    pair.Value.gameObject.SetActive(false);
                }
            }
        }

        public void RaiseEvent(HUB_EVENT_TYPE eventType, object[] data = null)
        {
            foreach (var pair in _menus)
            {
                pair.Value.OnEvent(eventType, data);
            }
        }

        public void OpenMenu<T>(object data = null)
            where T : OpenableHubMenu
        {
            OpenMenu(typeof(T), data);
        }

        public void OpenMenu(Type menuType, object data = null)
        {
            if (!menuType.IsSubclassOf(typeof(OpenableHubMenu)))
            {
                return;
            }

            OpenableHubMenu menu = _menus[menuType] as OpenableHubMenu;
            if (menu == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"No menu with type {menuType.FullName}");
#endif
                return;
            }

            if (menu.IsAdditional)
            {
                menu.Show(data);
            }
            else
            {
                if (_openedMenu != menu)
                {
                    CloseOpenedMenu();
                    _openedMenu = menu;
                }
                
                _openedMenu.Show(data);
            }
        }

        public void CloseOpenedMenu()
        {
            if (_openedMenu == null)
            {
                return;
            }

            _openedMenu.Hide();
            _openedMenu = null;
        }

        public OpenableHubMenu GetOpenedMenu() => _openedMenu;

        public bool IsTypeOfOpenedMenu(Type type)
        {
            return type != null && _openedMenu != null && _openedMenu.GetType().IsSubclassOf(type);
        }

        public bool IsTypeOfOpenedMenu<T>()
            where T : OpenableHubMenu
        {
            return _openedMenu is T;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseOpenedMenu();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _localization.ChangeLanguage();
            }
        }

        public void CloseMenu<T>() where T : OpenableHubMenu
        {
            CloseMenu(typeof(T));
        }

        public void CloseMenu(Type menuType)
        {
            if (!menuType.IsSubclassOf(typeof(OpenableHubMenu)))
            {
                return;
            }

            OpenableHubMenu menu = _menus[menuType] as OpenableHubMenu;
            if (menu == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"No menu with type {menuType.FullName}");
#endif
                return;
            }

            menu.Hide();
            if (_openedMenu == menu)
            {
                _openedMenu = null;
            }
        }
    }
}
