using UnityEngine;
using UnityEngine.UI;

namespace RollABall.Licensing
{
    public class LicenseMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainMenuRoot;
        public GameObject activationRoot;

        [Header("Activation UI")]
        public Text machineCodeText;
        public InputField activationKeyInput;
        public Text statusText;

        private void Start()
        {
            if (machineCodeText != null)
            {
                machineCodeText.text = LicenseService.GetMachineCode();
            }

            Refresh();
        }

        public void Refresh()
        {
            bool activated = LicenseService.IsActivated(out string reason);

            if (mainMenuRoot != null)
            {
                mainMenuRoot.SetActive(activated);
            }

            if (activationRoot != null)
            {
                activationRoot.SetActive(!activated);
            }

            if (statusText != null)
            {
                statusText.text = activated ? "Activated" : reason;
            }
        }

        public void OnCopyMachineCode()
        {
            GUIUtility.systemCopyBuffer = LicenseService.GetMachineCode();
            if (statusText != null)
            {
                statusText.text = "Machine code copied.";
            }
        }

        public void OnActivate()
        {
            string key = activationKeyInput != null ? activationKeyInput.text : string.Empty;
            if (!LicenseService.TryValidateActivationKey(key, out string reason))
            {
                if (statusText != null)
                {
                    statusText.text = reason;
                }
                return;
            }

            LicenseService.SaveActivationKey(key.Trim());
            Refresh();
        }

        public void OnClearActivation()
        {
            LicenseService.ClearActivationKey();
            if (activationKeyInput != null)
            {
                activationKeyInput.text = string.Empty;
            }
            Refresh();
        }
    }
}
