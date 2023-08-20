# cmd_Civil3D 🗺️
Custom commands written for Civil 3D.

Working Commands:
- ADDVALUETOSINGLENUMBER
- FINDENTITYTYPE

For now, all commands should work in AutoCAD too.

## ADDVALUETOSINGLENUMBER
This should allow to select an MText or DBText to add a specified value. Let's say, if the specified value is 0.1 and then the selected text says 10.3, then it will be updated to 10.4. 

It only works if the text is a number and does not contain other letters. Selecting a "13.5" text would work but selecting a "E:13.4" would not work.

Command allows to select a color parameter optionally. This will change the color of the text that has been updated.

Mode parameter exists to change between single and multiple selection but only single selection is working for now.

## FINDENTITYTYPE
It return the entity type of the selected object. Not much use, only exists to find the object in the API documentation.
