using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CardCollection {
    public class CardEntry {
        public CardSO cardSO;
        public int quantity;
    }

    private ResourceManager resourceManager;

    public CardCollection(ResourceManager resourceManager) {
        this.resourceManager = resourceManager;
    }

    public List<CardEntry> cardEntries = new List<CardEntry>();

    // ��������� ����� � �������� � ������ �������
    private void AddCardToCollection(CardSO card) {
        // �������� �� ����� ��� � � ��������
        CardEntry existingEntry = cardEntries.Find(entry => entry.cardSO == card);
        if (existingEntry != null) {
            existingEntry.quantity++;
        } else {
            cardEntries.Add(new CardEntry { cardSO = card, quantity = 1 });
        }
    }

    public void GenerateTestDeck(int count) {
        
        for (int i = 0; i < count; i++) {
            AddCardToCollection(resourceManager.GetRandomCard());
        }
    }

    // ��������� �������� �� ����� ���������
    public void GenerateCollection(float averageCost, int count, float strength) {
        // ������ ��� ��������� ��� ����, �� �������� �� ������ � �������
        List<CardSO> validCards = new List<CardSO>();

        // ������ �� ����� � ���� � validCards, ���������� �� �������
        foreach (CardSO card in resourceManager.GetAllCards()) {
            if (IsValidCard(card, averageCost)) {
                validCards.Add(card);
            }
        }

        if (validCards.Count == 0) return;

        // ��������� ��������
        cardEntries.Clear(); // ������� �������� ����� ����������

        for (int i = 0; i < count; i++) {
            CardSO card = GetRandomCardByRarity(strength, validCards);
            AddCardToCollection(card);
        }
    }

    // ��������, �� ����� �������� �� �������
    private bool IsValidCard(CardSO card, float averageCost) {
        float deviation = 0.4f * averageCost; // ³�������� � ����� 40%
        return card.cost >= (averageCost - deviation) && card.cost <= (averageCost + deviation);
    }

    // ���� ��������� ����� � ����������� strength (������)
    private CardSO GetRandomCardByRarity(float strength, List<CardSO> validCards) {

        float rarityChance = UnityEngine.Random.Range(0f, 1f);

        Rarity selectedRarity = rarityChance < strength
            ? GetRarityBasedOnStrength(strength)
            : Rarity.Common;

        // Գ������� ����� �� ������
        List<CardSO> filteredCards = validCards.FindAll(card => card.rarity == selectedRarity);

        // ���������� �������� ����
        if (filteredCards.Count > 0) {
            return filteredCards[UnityEngine.Random.Range(0, filteredCards.Count)];
        } else if (validCards.Count > 0) {
            return validCards[UnityEngine.Random.Range(0, validCards.Count)];
        } else {
            throw new InvalidOperationException("No valid cards available to select.");
        }
    }


    // ������� ������ ����� � ��������� �� strength
    private Rarity GetRarityBasedOnStrength(float value) {
        if (value < 0 || value > 1) {
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 1.");
        }

        var values = Enum.GetValues(typeof(Rarity));
        int enumCount = values.Length;
        if (enumCount == 0) {
            throw new InvalidOperationException("Enum must have at least one value.");
        }
        int index = (int)Math.Round(value * (enumCount - 1));

        // �������� �� �������, ���� index ������ �� ��� ������ (���� Math.Round ����� �� ������� ���������)
        if (index < 0) {
            index = 0;
        } else if (index >= enumCount) {
            index = enumCount - 1;
        }
        return (Rarity)values.GetValue(index);
    }
}
