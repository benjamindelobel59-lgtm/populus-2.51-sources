package com.ankamagames.berilia.types.graphic
{
   import com.ankamagames.berilia.types.data.RadioGroup;
   import com.ankamagames.berilia.types.data.UiData;
   import com.ankamagames.berilia.types.data.UiModule;
   import flash.display.DisplayObjectContainer;
   
   public class UiRootContainer extends GraphicContainer
   {
      
      public function UiRootContainer()
      {
         super();
      }
      
      public function get windowOwner() : Object
      {
         return new Object();
      }
      
      public function set windowOwner(v:Object) : void
      {
      }
      
      public function get uiClass() : *
      {
         return null;
      }
      
      public function set uiClass(v:*) : void
      {
      }
      
      public function get uiModule() : UiModule
      {
         return new UiModule();
      }
      
      public function set uiModule(v:UiModule) : void
      {
      }
      
      public function get strata() : int
      {
         return new int();
      }
      
      public function set strata(v:int) : void
      {
      }
      
      public function get depth() : int
      {
         return new int();
      }
      
      public function set depth(v:int) : void
      {
      }
      
      public function get scalable() : Boolean
      {
         return new Boolean();
      }
      
      public function set scalable(v:Boolean) : void
      {
      }
      
      public function get modal() : Boolean
      {
         return new Boolean();
      }
      
      public function set modal(v:Boolean) : void
      {
      }
      
      public function get giveFocus() : Boolean
      {
         return new Boolean();
      }
      
      public function set giveFocus(v:Boolean) : void
      {
      }
      
      public function get modalIndex() : uint
      {
         return new uint();
      }
      
      public function set modalIndex(v:uint) : void
      {
      }
      
      public function get radioGroup() : Array
      {
         return new Array();
      }
      
      public function set radioGroup(v:Array) : void
      {
      }
      
      public function get cached() : Boolean
      {
         return new Boolean();
      }
      
      public function set cached(v:Boolean) : void
      {
      }
      
      public function get hideAfterLoading() : Boolean
      {
         return new Boolean();
      }
      
      public function set hideAfterLoading(v:Boolean) : void
      {
      }
      
      public function get transmitFocus() : Boolean
      {
         return new Boolean();
      }
      
      public function set transmitFocus(v:Boolean) : void
      {
      }
      
      public function get setOnTopOnClick() : Boolean
      {
         return new Boolean();
      }
      
      public function set setOnTopOnClick(v:Boolean) : void
      {
      }
      
      public function get setOnTopBeforeMe() : Array
      {
         return new Array();
      }
      
      public function set setOnTopBeforeMe(v:Array) : void
      {
      }
      
      public function get setOnTopAfterMe() : Array
      {
         return new Array();
      }
      
      public function set setOnTopAfterMe(v:Array) : void
      {
      }
      
      public function get constants() : Array
      {
         return new Array();
      }
      
      public function set constants(v:Array) : void
      {
      }
      
      public function get tempHolder() : DisplayObjectContainer
      {
         return new DisplayObjectContainer();
      }
      
      public function set tempHolder(v:DisplayObjectContainer) : void
      {
      }
      
      public function get restoreSnapshotAfterLoading() : Boolean
      {
         return new Boolean();
      }
      
      public function set restoreSnapshotAfterLoading(v:Boolean) : void
      {
      }
      
      public function get parentUiRoot() : UiRootContainer
      {
         return new UiRootContainer();
      }
      
      public function set parentUiRoot(v:UiRootContainer) : void
      {
      }
      
      public function get childUiRoot() : UiRootContainer
      {
         return new UiRootContainer();
      }
      
      public function set childUiRoot(v:UiRootContainer) : void
      {
      }
      
      public function get subHintContainer() : GraphicContainer
      {
         return new GraphicContainer();
      }
      
      public function set subHintContainer(v:GraphicContainer) : void
      {
      }
      
      public function get fullscreen() : Boolean
      {
         return false;
      }
      
      public function set fullscreen(value:Boolean) : void
      {
      }
      
      public function get properties() : *
      {
         return null;
      }
      
      public function set properties(o:*) : void
      {
      }
      
      public function set useCustomSize(b:Boolean) : void
      {
      }
      
      public function get useCustomSize() : Boolean
      {
         return false;
      }
      
      public function set disableRender(b:Boolean) : void
      {
      }
      
      public function get disableRender() : Boolean
      {
         return false;
      }
      
      public function get ready() : Boolean
      {
         return false;
      }
      
      public function set modalContainer(val:GraphicContainer) : void
      {
      }
      
      public function set showModalContainer(val:Boolean) : void
      {
      }
      
      public function get uiData() : UiData
      {
         return null;
      }
      
      public function get scriptTime() : Number
      {
         return 0;
      }
      
      public function get childIndex() : int
      {
         return 0;
      }
      
      public function set childIndex(pChildIndex:int) : void
      {
      }
      
      public function get magneticElements() : Array
      {
         return null;
      }
      
      public function addElement(sName:String, oElement:Object) : void
      {
      }
      
      public function removeElement(sName:String) : void
      {
      }
      
      public function getElement(sName:String) : GraphicContainer
      {
         return null;
      }
      
      public function getElements() : Array
      {
         return null;
      }
      
      public function registerHintContainer(oElement:Object) : void
      {
      }
      
      public function getHintContainer() : GraphicContainer
      {
         return null;
      }
      
      public function getConstant(name:String) : *
      {
         return null;
      }
      
      public function iAmFinalized(target:Object) : void
      {
      }
      
      public function render() : void
      {
      }
      
      public function registerMagneticElement(gc:GraphicContainer) : void
      {
      }
      
      public function removeMagneticElement(gc:GraphicContainer) : void
      {
      }
      
      public function registerId(sName:String, geReference:GraphicElement) : void
      {
      }
      
      public function deleteId(sName:String) : void
      {
      }
      
      public function getElementById(sName:String) : GraphicElement
      {
         return null;
      }
      
      public function removeFromRenderList(sName:String) : void
      {
      }
      
      public function addDynamicSizeElement(geReference:GraphicElement) : void
      {
      }
      
      public function addDynamicElement(ge:GraphicElement) : void
      {
      }
      
      public function addPostFinalizeComponent(fc:Object) : void
      {
      }
      
      public function addFinalizeElement(fc:Object) : void
      {
      }
      
      public function resetSizeAndPosition() : void
      {
      }
      
      public function addRadioGroup(groupName:String) : RadioGroup
      {
         return null;
      }
      
      public function getRadioGroup(name:String) : RadioGroup
      {
         return null;
      }
      
      public function addLinkedUi(uiName:String) : void
      {
      }
      
      public function removeLinkedUi(uiName:String) : void
      {
      }
      
      public function updateLinkedUi() : void
      {
      }
      
      public function call(fct:Function, args:Array, accesKey:Object) : void
      {
      }
      
      public function destroyUi() : void
      {
      }
      
      public function makeSnapshot() : void
      {
      }
      
      public function setOnTop() : void
      {
      }
      
      public function processLocation(geElem:GraphicElement, fromRender:Boolean = false) : void
      {
      }
   }
}

