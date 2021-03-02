## Week5

### Cursor System

#### Game Architecture Changes

- Add folder *Effects/cursor*

- Add Script */GameManager/CursorController*

#### Current Cursor Sprites

- default_cursor *(./Sprite/Cursor/default_cursor)*: default cursor in scene
- point_cursor *(./Sprite/Cursor/point_cursor)*:  when mouse stops on the normal architectures, cursor changes to point_cursor
- attack_cursor *(./Sprite/Cursor/attack_cursor.png)*: when mouse stops on the attacked architectures, cursor changes to attack_cursor
- Efx_Click_Green *(./Effects/Cursor/Effect_Prefeb/Efx_Click_Green.prefab)*: cursor effect when click the player movement target position

#### Current Cursor Trigger GameObjects

- Ground: Efx_Click_Green
- Tower: attack_cursor

#### Game Setting Changes

- Add a *Cursor* GameObject

- Add *Confiner* Tag and assign it to CM confiner

  (Basically use CMconfiner as a 2Dcollider to detect collision)

- Add a 2D Collider to MidTower Prefab



### Minimap System

#### Game Architecture Changes

- Add *Material/MinimapRenderTexture*
- Add Script */MinimapCameraFollow*

#### Current Minimap Prefabs

- MinimapCamera *(./Prefab/MinimapCamera.prefab)*
- MinimapWindow *(./Prefab/Minimap/MinimapWindow.prefab)*
  - MinimapBorder
    - MinimapRenderer
- MiniIconWorrior *(./Prefab/Minimap/MiniIconWorrior.prefab)*
- MiniIconTower *(./Prefab/Minimap/MiniIconTower.prefab)*
- MiniIconDie *(./Prefab/Minimap/MiniIconDie.prefab)*

#### Current displayed GameObjects on Minimap

- Player
  - MiniIconWorrior
  - MiniIconDie
- Tower
  - MiniIconTower

Currently, players move on the minimap displayed as red spot and tower as yellow spot. Player's dead will trigger an Instantiate error sign prefab to mark dead location.

#### Game Setting Changes

- Add a *MinimapCamera* to *Player_Isometric_Worrior*
- Add *MinimapWindow* to *Player_Isometric_Worrior/MinimapCanvas*
- Add *MiniIconWorrior* to *Player_Isometric_Worrior*
- Add *MiniIconTower* to *MidTower*
- Add *Minimap* and *Ground* Layer

#### Note

##### Render camera to canvas

Assign *MinimapRenderTexture* to MinimapCamera/Target Texture

Assign *MinimapRenderTexture* to MinimapRenderer/Raw Image/Texture

##### Layer Setting

Assign all minimap prefabs to Minimap Layer

In minimap camera, set Camera/Culling Mask to Minimap. Then we will only see minimap icons in this camera.

In main camara, we need to unselet Minimap Layer, so all those icons would not appear in main camera.

##### (DELETED)UI Mask

A UI Image/RawImage with a mask applied conforms to the shape of that Mask.

MinimapWindow is a circle image with Mask component. The MinimapRenderder is a UI RawImage and suppose to be squre, but it's masked to its parent UI and conformed to a circle.



### Map System

Order of single tile: E -> N -> S -> W



Tilemap Renderer Sorting Layer:

- Level -1: MapBorderCollider
- Level 0: Ground
- Level 1-5: Map Levels



Add Background: the same color as Ground border color.

- Layer: Ground

- Render Layer: Level -2



### Need to fix

1. map collider
2. chat