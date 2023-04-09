using System;
using System.Collections;
using System.IO;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.UI;
using UnityEngine.Events;
using VRC.Core;
using VRC.UI.Elements.Controls;
using VRC.UI.Elements.Menus;
using Action = System.Action;
using IList = Il2CppSystem.Collections.IList;

namespace AvatarSearcher
{
    public class AvatarSearch : MelonMod
    {
        private static GameObject categoryButton;
        private static GameObject favoriteCategoryButton;
        private static System.Collections.Generic.List<Avatar> avatarList;
        private static System.Collections.Generic.List<Avatar> favoriteList;
        private static ObjectPublic1ILLiOb1InILLiStInUnique<IList> shownAvatars;
        private static GameObject mainMenu;

        public override void OnInitializeMelon()
        {           
            MelonCoroutines.Start(WaitForMain());
            base.OnInitializeMelon();
        }

        public static IEnumerator WaitForMain()
        {
            while (Object.FindObjectOfType<VRC.UI.Elements.MainMenu>() == null) yield return null;

            mainMenu = Object.FindObjectOfType<VRC.UI.Elements.MainMenu>().gameObject;
            Patches.OnInitialize();

            GameObject buttonTemplate = mainMenu.transform.Find(
                "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/Viewport/VerticalLayoutGroup/VerticalLayoutGroup User/Cell_MM_SidebarListItem (Favorites)").gameObject;
            mainMenu.transform.Find(
                    "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/Viewport/VerticalLayoutGroup/VerticalLayoutGroup Dynamic")
                .gameObject.active = false;

            var parent = buttonTemplate.transform.parent;
            categoryButton = Object.Instantiate(buttonTemplate, parent);
            categoryButton.name = "Cell_MM_SidebarListItem (Avatar Search)";
            categoryButton.SetActive(true);
            categoryButton.transform.Find("Mask/Text_Name").gameObject.GetComponent<TextMeshProUGUIEx>().text = "Avatar Search";
            categoryButton.GetComponent<DataContextSelectorButton>().field_Public_String_0 = "Avatar Search";
            categoryButton.GetComponent<DataContextSelectorButton>().field_Public_Object_0 =
                new ValueTypePublicSealed1StObInObObUnique();
            categoryButton.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>().field_Public_String_0 = "Avatar Search results";
            categoryButton.transform.Find("Icon").gameObject.GetComponent<Image>().overrideSprite = mainMenu.transform
                .Find("Container/PageButtons/HorizontalLayoutGroup/Page_Search/Icon").gameObject.GetComponent<Image>()
                .sprite;
            categoryButton.transform.Find("Count_BG/Text_Number").gameObject.GetComponent<TextMeshProUGUIEx>().text = "0";

            favoriteCategoryButton = Object.Instantiate(buttonTemplate, parent);
            favoriteCategoryButton.name = "Cell_MM_SidebarListItem (Extra Favorites)";
            favoriteCategoryButton.SetActive(true);
            favoriteCategoryButton.transform.Find("Mask/Text_Name").gameObject.GetComponent<TextMeshProUGUIEx>().text = "Extra Favorites";
            favoriteCategoryButton.GetComponent<DataContextSelectorButton>().field_Public_String_0 = "Extra Favorites";
            favoriteCategoryButton.GetComponent<DataContextSelectorButton>().field_Public_Object_0 =
            new ValueTypePublicSealed1StObInObObUnique();
            favoriteCategoryButton.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>().field_Public_String_0 = "Extra Favorites";

            InitializeFavorites();

            GameObject searchButtonTemplate = mainMenu.transform
                .Find(
                    "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Panel_SelectedAvatar/ScrollRect/Viewport/VerticalLayoutGroup/Button_AddToFavorites")
                .gameObject;

            GameObject searchButton = Object.Instantiate(searchButtonTemplate, searchButtonTemplate.transform.parent);
            searchButton.name = "Button_Search";
            searchButton.SetActive(true);
            while (searchButton.GetComponentInChildren<TextMeshProUGUIEx>() == null) yield return null;
            searchButton.GetComponentInChildren<TextMeshProUGUIEx>().text = "Fav/UnFav Avatar";
            searchButton.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>().field_Public_String_0 = "Fav/UnFav Avatar";

            searchButton.transform.Find("Text_ButtonName/FavoriteIcon").gameObject.GetComponent<Image>().overrideSprite =
                buttonTemplate.transform.Find("Icon").gameObject.GetComponent<Image>().sprite;

            searchButton.transform.Find("Text_ButtonName/FavoriteIcon").gameObject.SetActive(true);
            searchButton.transform.Find("Text_ButtonName/FavoriteIcon").name = "SearchIcon";

            Object.Destroy(searchButton.transform.Find("Text_ButtonName/UnfavoriteIcon").gameObject);
            Object.Destroy(searchButton.transform.Find("Text_ButtonName/RemoveIcon").gameObject);
            Object.Destroy(searchButton.transform.Find("Badge").gameObject);

            GameObject favoriteButtonTemplate = mainMenu.transform.Find(
                "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Header_MM_H2/RightItemContainer/ToggleCellSize").gameObject;

            GameObject favoriteButton = Object.Instantiate(favoriteButtonTemplate, favoriteButtonTemplate.transform.parent);
            favoriteButton.name = "Button_Favorite";
            favoriteButton.SetActive(true);
            favoriteButton.transform.Find("Text_ButtonName/Icon").gameObject.GetComponent<Image>().overrideSprite = mainMenu.transform
                .Find("Container/PageButtons/HorizontalLayoutGroup/Page_Search/Icon").gameObject.GetComponent<Image>()
                .sprite;
            favoriteButton.GetComponent<VRC.UI.Elements.Tooltips.UiTooltip>().field_Public_String_0 = "Add/Remove selected avatar to/from favorites";

            mainMenu.transform.Find(
                "Container/MMParent/Menu_Avatars").gameObject.GetComponent<MainMenuAvatars>()._buttonGroup._buttons.Add(categoryButton.GetComponent<SidebarListItem>());
            mainMenu.transform.Find(
                "Container/MMParent/Menu_Avatars").gameObject.GetComponent<MainMenuAvatars>()._buttonGroup.field_Private_Dictionary_2_String_DataContextSelectorButton_0.Add("Avatar Search", categoryButton.GetComponent<SidebarListItem>());

            var buttons = mainMenu.transform.Find(
                    "Container/MMParent/Menu_Avatars").gameObject.GetComponent<MainMenuAvatars>()._buttons
                .AddItem(categoryButton.GetComponent<SidebarListItem>());

            var obj = categoryButton.GetComponent<SidebarListItem>().field_Public_MonoBehaviour_0.gameObject;

            shownAvatars = obj.GetComponent<AvatarContentSection>().field_Private_ObjectPublic1ILLiOb1InILLiStInUnique_1_IList_0;

            while (shownAvatars == null)
            {
                shownAvatars = obj.GetComponent<AvatarContentSection>().field_Private_ObjectPublic1ILLiOb1InILLiStInUnique_1_IList_0;
                yield return null;
            }

            while (obj.GetComponent<AvatarContentSection>().enabled)
            {
                obj.GetComponent<AvatarContentSection>().enabled = false;
                yield return null;
            }

            mainMenu.transform.Find(
                    "Container/MMParent/Menu_Avatars").gameObject.GetComponent<MainMenuAvatars>()._buttons
                .AddItem(categoryButton.GetComponent<SidebarListItem>());

            searchButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();

            favoriteButton.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();

           

            void SearchAvatar(string s, List<KeyCode> k, Text t)
            {
                SearchAvatars(s);
            }

            AddAction(favoriteButton, () =>
            {
                mainMenu.transform.Find(
                    "Container/MMParent/Menu_Avatars").gameObject.GetComponent<DataContextSelectorGroup>().Method_Public_Void_DataContextSelectorButton_0(categoryButton.GetComponent<DataContextSelectorButton>());
                // replace with new keyboard at some point
               
                    VRCUiPopupManager.prop_VRCUiPopupManager_0.Method_Public_Void_String_String_InputType_Boolean_String_Action_3_String_List_1_KeyCode_Text_Action_String_Boolean_Action_1_VRCUiPopup_Boolean_Int32_0(
                        "Avatar Search", "", TMP_InputField.InputType.Standard, false, "Search", (System.Action<string, List<KeyCode>, Text>)SearchAvatar, null);
                
                
            });

            AddAction(searchButton, () =>
            {
                AddFavorite(mainMenu.transform.Find("Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Panel_SelectedAvatar/Panel_MM_AvatarViewer/Avatar")
                    .GetComponent<VRC.SimpleAvatarPedestal>().field_Internal_ApiAvatar_0);
            });
        }

        private static void SearchAvatars(string s)
        {
            avatarList = SARSUtils.Search(s);
            MelonCoroutines.Start(LoadAvatars());
        }

        public static IEnumerator LoadAvatars(bool wait = false, bool changeTitle = false)
        {
            if (changeTitle)
                mainMenu.transform.Find("Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Header_MM_H2/LeftItemContainer/Text_Title")
                    .gameObject.GetComponent<TextMeshProUGUIEx>().text = "Avatar Search";
            if (avatarList == null) yield break;
            shownAvatars.field_Private_List_1_Object_0.Clear();

            if (wait)
                yield return new WaitForSecondsRealtime(0.2f);

            foreach (Avatar aresAvi in avatarList)
            {
                var avi = new Object1PublicOb1BoObStBoDaStBo1Unique();

                var apiAvi = new ApiAvatar();
                apiAvi._id_k__BackingField = aresAvi.avatarId;
                apiAvi.id = aresAvi.avatarId;
                apiAvi.thumbnailImageUrl = aresAvi.thumbnailUrl;
                apiAvi._thumbnailImageUrl_k__BackingField =
                    aresAvi.thumbnailUrl;
                apiAvi.description = aresAvi.avatarDescription;
                apiAvi._description_k__BackingField = aresAvi.avatarDescription;
                apiAvi.name = aresAvi.avatarName;
                apiAvi._name_k__BackingField = aresAvi.avatarName;
                apiAvi.authorName = aresAvi.authorName;
                apiAvi._authorName_k__BackingField = aresAvi.authorName;
                apiAvi.authorId = aresAvi.authorId;
                apiAvi._authorId_k__BackingField = aresAvi.authorId;
                apiAvi.imageUrl = aresAvi.imageUrl;
                apiAvi._imageUrl_k__BackingField =
                    aresAvi.imageUrl;

                avi.field_Protected_TYPE_0 = apiAvi;

                shownAvatars.field_Private_List_1_Object_0.Add(avi);
            }

            categoryButton.transform.Find("Count_BG/Text_Number").gameObject.GetComponent<TextMeshProUGUIEx>().text =
                $"{avatarList.Count}";

            //moves the layout group a bit to update the thing and show the avatars without user interaction
            if (wait)
                yield return new WaitForSecondsRealtime(0.1f);
            else
                yield return new WaitForSecondsRealtime(0.3f);

            mainMenu.transform
                .Find(
                    "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Viewport/VerticalLayoutGroup")
                .transform.Translate(Vector3.up * 0.1f);

            yield return new WaitForSecondsRealtime(0.2f);

            mainMenu.transform
                .Find(
                    "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Viewport/VerticalLayoutGroup")
                .transform.Translate(Vector3.up * -0.1f);
        }

        private static void InitializeFavorites()
        {
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "UserData", "AvatarSearch")))
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "UserData", "AvatarSearch"));

            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "UserData", "AvatarSearch/favorites.json")))
                favoriteList =
                    JsonConvert.DeserializeObject<System.Collections.Generic.List<Avatar>>(
                        File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "UserData", "AvatarSearch/favorites.json")));
            else
            {
                favoriteList = new System.Collections.Generic.List<Avatar>();
                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "UserData", "AvatarSearch/favorites.json"), JsonConvert.SerializeObject(favoriteList));
            }
            favoriteCategoryButton.transform.Find("Count_BG/Text_Number").gameObject.GetComponent<TextMeshProUGUIEx>().text =
                $"{favoriteList?.Count}";
        }

        public static void AddFavorite(ApiAvatar avatar)
        {
            Avatar avi = new Avatar();
            avi.avatarDescription = avatar.description;
            avi.authorName = avatar.authorName;
            avi.authorId = avatar.authorId;
            avi.avatarId = avatar.id;
            avi.avatarName = avatar.name;
            avi.imageUrl = avatar.imageUrl;
            avi.thumbnailUrl = avatar.thumbnailImageUrl;
            foreach (Avatar av in favoriteList)
            {
                if (av.avatarId == avi.avatarId)
                {
                    favoriteList.Remove(av);
                    return;
                }
            }
            favoriteList.Add(avi);
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "UserData", "AvatarSearch/favorites.json"), JsonConvert.SerializeObject(favoriteList));
            favoriteCategoryButton.transform.Find("Count_BG/Text_Number").gameObject.GetComponent<TextMeshProUGUIEx>().text =
                $"{favoriteList.Count}";
        }

        public static IEnumerator LoadFavorites()
        {
            mainMenu.transform
                .Find(
                    "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Header_MM_H2/LeftItemContainer/Text_Title")
                .gameObject.GetComponent<TextMeshProUGUIEx>().text = "Extra Favorites";
            if (favoriteList == null) yield break;
            shownAvatars.field_Private_List_1_Object_0.Clear();

            yield return new WaitForSecondsRealtime(0.2f);

            foreach (Avatar aresAvi in favoriteList)
            {
                var avi = new Object1PublicOb1BoObStBoDaStBo1Unique();

                var apiAvi = new ApiAvatar();
                apiAvi._id_k__BackingField = aresAvi.avatarId;
                apiAvi.id = aresAvi.avatarId;
                apiAvi.thumbnailImageUrl = aresAvi.thumbnailUrl;
                apiAvi._thumbnailImageUrl_k__BackingField =
                    aresAvi.thumbnailUrl;
                apiAvi.description = aresAvi.avatarDescription;
                apiAvi._description_k__BackingField = aresAvi.avatarDescription;
                apiAvi.name = aresAvi.avatarName;
                apiAvi._name_k__BackingField = aresAvi.avatarName;
                apiAvi.authorName = aresAvi.authorName;
                apiAvi._authorName_k__BackingField = aresAvi.authorName;
                apiAvi.authorId = aresAvi.authorId;
                apiAvi._authorId_k__BackingField = aresAvi.authorId;
                apiAvi.imageUrl = aresAvi.imageUrl;
                apiAvi._imageUrl_k__BackingField =
                    aresAvi.imageUrl;

                avi.field_Protected_TYPE_0 = apiAvi;

                shownAvatars.field_Private_List_1_Object_0.Add(avi);
            }

            favoriteCategoryButton.transform.Find("Count_BG/Text_Number").gameObject.GetComponent<TextMeshProUGUIEx>().text =
                $"{favoriteList.Count}";

            //moves the layout group a bit to update the thing and show the avatars without user interaction
            yield return new WaitForSecondsRealtime(0.2f);

            mainMenu.transform
                .Find(
                    "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Viewport/VerticalLayoutGroup")
                .transform.Translate(Vector3.up * 0.1f);

            yield return new WaitForSecondsRealtime(0.2f);

            mainMenu.transform
                .Find(
                    "Container/MMParent/Menu_Avatars/Menu_MM_DynamicSidePanel/Panel_SectionList/ScrollRect_Navigation/ScrollRect_Content/Viewport/VerticalLayoutGroup")
                .transform.Translate(Vector3.up * -0.1f);
        }

        private static void AddAction(GameObject button, Action action)
        {
            if (action != null)
                button.GetComponent<Button>().onClick.AddListener(UnhollowerRuntimeLib.DelegateSupport.ConvertDelegate<UnityAction>(action));
        }
    }
}
