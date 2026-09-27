using UnityEngine;
using UnityEngine.UI;
using RogueLike.Core;
using RogueLike.Managers;

namespace RogueLike.UI
{
    public class LevelUpSelectionUI : MonoBehaviour
    {
        [SerializeField] private RunManager runManager;
        [SerializeField] private GameObject panel;
        [SerializeField] private Button[] choiceButtons;
        [SerializeField] private Text[] choiceLabels;

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            bool isLevelUp = state == GameState.LevelUp;
            if (panel != null)
            {
                panel.SetActive(isLevelUp);
            }

            if (isLevelUp)
            {
                ShowChoices();
            }
        }

        private void ShowChoices()
        {
            var choices = runManager.GetItemChoices();

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                bool hasChoice = i < choices.Length;
                choiceButtons[i].gameObject.SetActive(hasChoice);
                if (!hasChoice)
                {
                    continue;
                }

                var definition = choices[i];
                if (i < choiceLabels.Length && choiceLabels[i] != null)
                {
                    choiceLabels[i].text = definition.DisplayName;
                }

                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => runManager.ChooseItem(definition));
            }
        }
    }
}
