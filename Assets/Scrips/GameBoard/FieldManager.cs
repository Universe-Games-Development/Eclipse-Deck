using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class FieldManager : MonoBehaviour {
    private FieldOverseer _fieldOverseer;
    [SerializeField] private BoardSettings _boardSettings;

    private void DebugFields() {
    }

    private void Awake() {
        if (_boardSettings == null) {
            _boardSettings = new BoardSettings();
            _boardSettings.rowTypes[0] = FieldType.Attack;
            _boardSettings.rowTypes[1] = FieldType.Attack;
            _boardSettings.columns = 4;
        }
        _fieldOverseer = new FieldOverseer(_boardSettings);
    }

}