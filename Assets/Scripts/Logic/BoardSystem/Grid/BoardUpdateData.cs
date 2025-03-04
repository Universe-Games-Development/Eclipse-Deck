using System;
using System.Collections.Generic;

public class BoardUpdateData {
    public List<GridUpdateData> gridsUpdateData = new();

    public GridUpdateData GetUpdateByGlobalDirection(Direction globalDirection) {
        // Initialize combined update data for the specified global direction
        GridUpdateData combinedUpdateData = new GridUpdateData(globalDirection);

        // Iterate through each grid update data
        foreach (var gridUpdate in gridsUpdateData) {
            // Check if the grid update's direction matches the global direction
            if (CompassUtil.BelongsToGlobalDirection(gridUpdate.direction, globalDirection)) {
                // Combine the update data
                combinedUpdateData.addedFields.AddRange(gridUpdate.addedFields);
                combinedUpdateData.markedEmpty.AddRange(gridUpdate.markedEmpty);
                combinedUpdateData.removedFields.AddRange(gridUpdate.removedFields);
            }
        }

        return combinedUpdateData;
    }

    public List<Field> GetAllAddedFields() {
        List<Field> allAddedFields = new List<Field>();
        foreach (var gridUpdate in gridsUpdateData) {
            allAddedFields.AddRange(gridUpdate.addedFields);
        }
        return allAddedFields;
    }

    public List<Field> GetAllEmptyFields() {
        List<Field> allEmptyFields = new List<Field>();
        foreach (var gridUpdate in gridsUpdateData) {
            allEmptyFields.AddRange(gridUpdate.markedEmpty);
        }
        return allEmptyFields;
    }

    public List<Field> GetAllRemovedFields() {
        List<Field> allRemovedFields = new List<Field>();
        foreach (var gridUpdate in gridsUpdateData) {
            allRemovedFields.AddRange(gridUpdate.removedFields);
        }
        return allRemovedFields;
    }
}
