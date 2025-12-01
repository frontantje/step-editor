# Sequence Step Editor

This project is a functional, data-persistent **Step Editor** built using Unity's UI Toolkit. It allows a user to define, reorder, and modify a sequence of steps, simulating an in-game programming or automation tool.

## Project Details

* **Unity Version:** **6000.2**
* **Targeted Platform:** Standalone (Editor)
* **UI System:** Unity UI Toolkit (UXML, USS, C#)

## Features

* **Data Persistence:** The sequence state (`List<StepData>`) is automatically saved to a JSON file (`StepSequence.json`) on every modification and loaded on startup.
* **Thread Safety:** The UI is initialized in the **`Awake()`** method, and the `UIDocument` asset is assigned manually via code. This prevents main thread loading errors that occur when assets are linked in the Inspector.
* **Data Binding:** Input changes are captured using `ChangeEvent` handlers that retrieve the correct data model using **`FindAncestorUserData<StepData>()`**
* **Custom Style:** Overrides default Unity styling via USS.

***

## How to Run

1.  Open the project in **Unity Editor (version 6000.2)**.
2.  Open the scene located at `Assets/Scenes/Main.unity`.
3.  Press the **Play button**.
4.  The Step Editor UI will appear in the Game View.

### How to Use

* **Add/Remove:** Click **"Add New Step"** or the **"X"** button on a row.
* **Reorder:** Click and drag anywhere on a list item row. The list will shift smoothly (**Animated Reorder Mode**).
* **Edit:** Type into the Title or Value fields. Data is saved upon losing focus (blurring).
