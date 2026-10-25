
using UnityEngine;

public class StoreNavigation : MonoBehaviour
{
    [Header("Store Screens")]
    [SerializeField] private GameObject shop;
    [SerializeField] private GameObject ammoScreen;
    [SerializeField] private GameObject gunSelection;
    [SerializeField] private GameObject back;

    public void OpenAmmoScreen()
    {
        ammoScreen.SetActive(true);
        ammoScreen.transform.SetAsLastSibling();

        gunSelection.SetActive(false);
        back.SetActive(false);
    }

    public void OpenGunSelection()
    {
        gunSelection.SetActive(true);
        gunSelection.transform.SetAsLastSibling();

        ammoScreen.SetActive(false);
        back.SetActive(true);
    }

    public void ReturnToShop()
    {
        gunSelection.SetActive(false);
        ammoScreen.SetActive(false);
        back.SetActive(false);

        shop.SetActive(true);
        shop.transform.SetAsLastSibling();
    }
}

