# Reliable Export CSV input

The reported defect is a postfix painting a button over DPA's already processed tab list. Reserve a 48-pixel footer by reducing the argument passed into DoWindowContents and draw the export button first in a Harmony prefix. Keep an 18-pixel outer margin. DPA navigation, graph and search retain their own space. Keep the settings fallback and existing CSV format.

Moving an overlay alone repeats the collision whenever content changes. Filtering individual mouse events leaves a visually overlapping UI and requires additional input state. Reserving a footer addresses both geometry and event order with one patch, without modifying measured methods or the DPA distribution. This is a small bug fix within the user's requested scope.

Verify real Harmony argument binding against a test-only window and fake UI boundary, including a click on a tab behind the old button, unchanged selection during export, navigation, repaint, resizing and nonzero origin. Build using existing public RimRef signatures; package only addon files. Running Unity in game remains separate from these checks.
