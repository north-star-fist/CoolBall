using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sergei.Safonov.Unity.UI
{

    public static class UIUtil
    {

        const int UiLayer = 5;

        static List<RaycastResult> s_raycastResults = new();

        public static bool IsPointerOverUI(Vector3 mousePosition)
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
            {
                position = mousePosition
            };

            EventSystem.current.RaycastAll(eventDataCurrentPosition, s_raycastResults);

            for (int i = 0; i < s_raycastResults.Count; ++i)
            {
                if (s_raycastResults[i].gameObject.layer == UiLayer)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
