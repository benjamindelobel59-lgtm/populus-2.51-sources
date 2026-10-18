package com.ankamagames.berilia.api
{
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.components.Texture;
   import com.ankamagames.berilia.types.data.UiModule;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.berilia.types.graphic.UiRootContainer;
   import flash.display.DisplayObject;
   import flash.geom.Point;
   import flash.geom.Rectangle;
   
   public class UiApi
   {
      
      public function UiApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function loadUi(name:String, instanceName:String = null, params:* = null, strata:uint = 1, cacheName:String = null, replace:Boolean = false, externalUi:Boolean = false, restoreSnapshot:Boolean = true) : Object
      {
         return null;
      }
      
      public function loadUiInside(name:String, container:GraphicContainer, instanceName:String = null, params:* = null) : Object
      {
         return null;
      }
      
      public function unloadUi(instanceName:String = null) : void
      {
      }
      
      public function getUi(instanceName:String) : *
      {
         return null;
      }
      
      public function getUiByName(uiName:String) : *
      {
         return null;
      }
      
      public function setUiStrata(uiName:String, strata:int) : void
      {
      }
      
      public function getUiInstances() : Object
      {
         return null;
      }
      
      public function getHud() : Array
      {
         return null;
      }
      
      public function getModule(moduleName:String) : UiModule
      {
         return null;
      }
      
      public function handleComponentRestaureState(cpt:GraphicContainer, properties:Array = null, resistLightReset:Boolean = true) : void
      {
      }
      
      public function setModuleEnable(id:String, b:Boolean) : void
      {
      }
      
      public function addChild(target:Object, child:Object) : void
      {
      }
      
      public function removeChild(target:Object, child:Object) : void
      {
      }
      
      public function addChildAt(target:Object, child:Object, index:int) : void
      {
      }
      
      public function me() : UiRootContainer
      {
         return null;
      }
      
      public function initDefaultBinds() : void
      {
      }
      
      public function addShortcutHook(shortcutName:String, hook:Function, pPriority:* = false) : void
      {
      }
      
      public function addComponentHook(target:GraphicContainer, hookName:String) : void
      {
      }
      
      public function removeComponentHook(target:GraphicContainer, hookName:String) : void
      {
      }
      
      public function bindApi(targetTexture:Texture, propertyName:String, value:*) : Boolean
      {
         return false;
      }
      
      public function resetUiSavedUserModification(name:String = null) : void
      {
      }
      
      public function createComponent(type:String, ... params) : GraphicContainer
      {
         return null;
      }
      
      public function createContainer(type:String, ... params) : *
      {
         return null;
      }
      
      public function createInstanceEvent(target:DisplayObject, instance:*) : Object
      {
         return null;
      }
      
      public function getEventClassName(event:String) : String
      {
         return null;
      }
      
      public function addInstanceEvent(event:Object) : void
      {
      }
      
      public function clearPositionCache(instanceName:String = null) : void
      {
      }
      
      public function createUri(uri:String, force:Boolean = false) : Object
      {
         return null;
      }
      
      public function showTooltip(data:*, target:*, autoHide:Boolean = false, name:String = "standard", point:uint = 0, relativePoint:uint = 2, offset:* = 3, tooltipMaker:String = null, script:Class = null, makerParam:Object = null, cacheName:String = null, mouseEnabled:Boolean = false, strata:int = 4, zoom:Number = 1, uiModuleName:String = "", showDirectionalArrow:Boolean = false, container:UiRootContainer = null) : void
      {
      }
      
      public function hideTooltip(name:String = null) : void
      {
      }
      
      public function textTooltipInfo(content:String, css:String = null, cssClass:String = null, maxWidth:int = 400) : Object
      {
         return null;
      }
      
      public function getRadioGroupSelectedItem(rgName:String, me:UiRootContainer) : Object
      {
         return null;
      }
      
      public function setRadioGroupSelectedItem(rgName:String, item:Object, me:UiRootContainer) : void
      {
      }
      
      public function keyIsDown(keyCode:uint) : Boolean
      {
         return false;
      }
      
      public function keyIsUp(keyCode:uint) : Boolean
      {
         return false;
      }
      
      public function convertToTreeData(array:*) : Object
      {
         return null;
      }
      
      public function setFollowCursorUri(uri:*, lockX:Boolean = false, lockY:Boolean = false, xOffset:int = 0, yOffset:int = 0, scale:Number = 1) : void
      {
      }
      
      public function getFollowCursorUri() : Object
      {
         return null;
      }
      
      public function endDrag() : void
      {
      }
      
      public function preloadCss(url:String) : void
      {
      }
      
      public function setLabelStyleSheet(label:Label, cssUrl:String) : void
      {
      }
      
      public function getMouseX() : int
      {
         return 0;
      }
      
      public function getMouseY() : int
      {
         return 0;
      }
      
      public function getMouseDown() : Boolean
      {
         return false;
      }
      
      public function getStartWidth() : int
      {
         return 0;
      }
      
      public function getStartHeight() : int
      {
         return 0;
      }
      
      public function getStageWidth() : int
      {
         return 0;
      }
      
      public function getStageHeight() : int
      {
         return 0;
      }
      
      public function getVisibleStageBounds() : Rectangle
      {
         return null;
      }
      
      public function getWindowWidth() : int
      {
         return 0;
      }
      
      public function getWindowHeight() : int
      {
         return 0;
      }
      
      public function getWindowScale() : Number
      {
         return 0;
      }
      
      public function setFullScreen(enabled:Boolean, onlyMaximize:Boolean = false) : void
      {
      }
      
      public function isFullScreen() : Boolean
      {
         return false;
      }
      
      public function isResetting() : Boolean
      {
         return false;
      }
      
      public function useIME() : Boolean
      {
         return false;
      }
      
      public function place(relativeTo:*, relativePoint:uint, point:uint, pOffset:int, showDirectionalArrow:Boolean = false, placedComponent:GraphicContainer = null) : void
      {
      }
      
      public function buildOrnamentTooltipFrom(pTexture:Texture, pTarget:Rectangle) : void
      {
      }
      
      public function getTextSize(pText:String, pCss:Object, pCssClass:String) : Rectangle
      {
         return null;
      }
      
      public function setComponentMinMaxSize(component:GraphicContainer, minSize:Point, maxSize:Point) : void
      {
      }
      
      public function isUiLoading(pUiName:String) : Boolean
      {
         return false;
      }
      
      public function getColor(pColor:String) : Object
      {
         return null;
      }
      
      public function replaceParams(text:String, params:Array, replace:String = "%") : String
      {
         return null;
      }
      
      public function replaceKey(text:String) : String
      {
         return null;
      }
      
      public function getText(key:String, ... params) : String
      {
         return null;
      }
      
      public function getTextFromKey(key:uint, replace:String = "%", ... params) : String
      {
         return null;
      }
      
      public function processText(str:String, gender:String, singular:Boolean = true) : String
      {
         return null;
      }
      
      public function decodeText(str:String, params:Array) : String
      {
         return null;
      }
   }
}

