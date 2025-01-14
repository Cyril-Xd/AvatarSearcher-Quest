using System;
using HarmonyLib;
using System.Reflection;
using VRC.UI.Elements.Controls;

namespace AvatarSearcher
{
    internal class Patches
    {
        private static readonly HarmonyLib.Harmony HarmonyInstance = new HarmonyLib.Harmony("avatar search");

        public static void OnInitialize()
        {
            HarmonyInstance.Patch(typeof(DataContextSelectorGroup).GetMethod("Method_Public_Void_DataContextSelectorButton_0"), null, 
                GetLocalPatch("OnCategoryButtonClicked", typeof(Patches)));
        }
        
        private static void OnCategoryButtonClicked(VRC.UI.Elements.Controls.DataContextSelectorButton __0)
        {
            if (__0.ToString().Contains("Cell_MM_SidebarListItem (Avatar Search)"))
            {
                AvatarSearch.LoadAvatars(true, true);
            }

            if (__0.ToString().Contains("Cell_MM_SidebarListItem (Extra Favorites)"))
            {
                AvatarSearch.LoadFavorites();
            }
        }
        
        public static HarmonyMethod GetLocalPatch(string name, Type type)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            return new HarmonyMethod(method);
        }
    }
}
