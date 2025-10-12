The goal was the learn about Unity and Game development process. For my mini game, it will be a drag and drop game. For now, I went with a tutorial on how we can design similar game on 2d space. I went with creating a sample project after I watched few youtube videos on game development process. 

**What This Game Does**  
A color-matching puzzle where players drag colored squares from the left panel to matching colored outlines in the center.  
**What I Learned**

1. Core Unity Concepts  
   GameObjects & Components \- Built game objects with Transform, Image, and custom script components  
   MonoBehaviour Scripts \- Created GameManager, DraggableItem, and DropZone scripts  
   Unity Lifecycle \- Used Awake() for initialization and Start() for game setup  
   Dynamic Object Creation \- Programmatically created UI elements at runtime  
2. UI System  
   Canvas & RectTransform \- Set up UI layouts with proper anchoring and positioning  
   Image Component \- Displayed colored squares and outlines  
   Event System \- Configured EventSystem and StandaloneInputModule for input handling  
   Canvas Scaler \- Made UI responsive with proper scaling settings  
3. Game Architecture  
   Component-Based Design \- Separated drag logic, drop validation, and game management  
   Event-Driven Programming \- Used Unity's event system for user interactions  
   Color Matching Logic \- Implemented validation for matching colors with tolerance  
   Debug Logging \- Added comprehensive logging for troubleshooting