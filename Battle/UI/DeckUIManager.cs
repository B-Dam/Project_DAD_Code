using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeckUIManager : MonoBehaviour
{
    public static DeckUIManager Instance { get; private set; }

    [Header("공용 뷰어 패널")]
    [SerializeField] private GameObject panel;     // 하나만
    [SerializeField] private Transform content;    // 하나만
    [SerializeField] private GameObject cardListPrefab;

    public bool isViewActive => panel.activeSelf;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        panel.SetActive(false);
    }

    void Update()
    {
        if (panel.activeSelf && Keyboard.current.escapeKey.wasPressedThisFrame)
            ClosePanel();
    }

    // Draw 버튼
    public void ShowDrawPile()
    {
        // 이미 열려 있으면 닫기
        if (panel.activeSelf)
        {
            ClosePanel();
            return;
        }

        Populate(HandManager.Instance.deck);
        panel.SetActive(true);
    }

    // Discard 버튼
    public void ShowDiscardPile()
    {
        // 이미 열려 있으면 닫기
        if (panel.activeSelf)
        {
            ClosePanel();
            return;
        }

        Populate(HandManager.Instance.discard);
        panel.SetActive(true);
    }

    private void Populate(List<CardData> list)
    {
        // 기존 지우고
        foreach (Transform t in content) Destroy(t.gameObject);

        // 새로 채워넣기
        foreach (var data in list)
        {
            var go = Instantiate(cardListPrefab, content);
            go.GetComponent<CardListItem>().SetData(data);
        }
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        foreach (Transform t in content) Destroy(t.gameObject);
    }
}