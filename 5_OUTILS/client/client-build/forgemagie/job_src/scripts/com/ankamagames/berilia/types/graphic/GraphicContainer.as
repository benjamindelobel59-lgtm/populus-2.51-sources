package com.ankamagames.berilia.types.graphic
{
   import flash.display.DisplayObject;
   import flash.display.Sprite;
   import flash.geom.ColorTransform;
   import flash.geom.Point;
   import flash.geom.Rectangle;
   
   public class GraphicContainer extends Sprite
   {
      
      public function GraphicContainer()
      {
         super();
      }
      
      public function get minSize() : GraphicSize
      {
         return new GraphicSize();
      }
      
      public function set minSize(v:GraphicSize) : void
      {
      }
      
      public function get maxSize() : GraphicSize
      {
         return new GraphicSize();
      }
      
      public function set maxSize(v:GraphicSize) : void
      {
      }
      
      public function get isMagnetic() : Boolean
      {
         return false;
      }
      
      public function set isMagnetic(value:Boolean) : void
      {
      }
      
      public function get dragSavePosition() : Boolean
      {
         return false;
      }
      
      public function set dragSavePosition(value:Boolean) : void
      {
      }
      
      public function get resizeController() : Boolean
      {
         return false;
      }
      
      public function set resizeController(value:Boolean) : void
      {
      }
      
      public function getResizeController() : Object
      {
         return null;
      }
      
      public function get resizeTarget() : String
      {
         return null;
      }
      
      public function set resizeTarget(id:String) : void
      {
      }
      
      public function get dragTarget() : String
      {
         return null;
      }
      
      public function set dragTarget(value:String) : void
      {
      }
      
      public function set dragBoundsContainer(id:String) : void
      {
      }
      
      public function get dragBoundsContainer() : String
      {
         return null;
      }
      
      public function set useDragMagnetism(b:Boolean) : void
      {
      }
      
      public function get useDragMagnetism() : Boolean
      {
         return false;
      }
      
      public function set restrictionFunctionName(funcName:String) : void
      {
      }
      
      public function get dragController() : Boolean
      {
         return false;
      }
      
      public function set dragController(value:Boolean) : void
      {
      }
      
      public function get notMovableUIUnderConditions() : Boolean
      {
         return false;
      }
      
      public function set notMovableUIUnderConditions(value:Boolean) : void
      {
      }
      
      public function get customUnicName() : String
      {
         return null;
      }
      
      public function set themeDataId(id:String) : void
      {
      }
      
      public function get themeDataId() : String
      {
         return null;
      }
      
      public function set dropValidator(dv:Function) : void
      {
      }
      
      public function get dropValidator() : Function
      {
         return null;
      }
      
      public function set removeDropSource(rds:Function) : void
      {
      }
      
      public function get removeDropSource() : Function
      {
         return null;
      }
      
      public function set processDrop(pd:Function) : void
      {
      }
      
      public function get processDrop() : Function
      {
         return null;
      }
      
      public function set sizeControler(b:Boolean) : void
      {
      }
      
      public function get finalized() : Boolean
      {
         return false;
      }
      
      public function set finalized(b:Boolean) : void
      {
      }
      
      public function get hintAvailable() : Boolean
      {
         return false;
      }
      
      public function set hintAvailable(value:Boolean) : void
      {
      }
      
      public function get isInstance() : Boolean
      {
         return false;
      }
      
      public function set isInstance(value:Boolean) : void
      {
      }
      
      public function finalize() : void
      {
      }
      
      public function focus() : void
      {
      }
      
      public function get hasFocus() : Boolean
      {
         return false;
      }
      
      public function get colorTransform() : ColorTransform
      {
         return null;
      }
      
      public function set colorTransform(ct:ColorTransform) : void
      {
      }
      
      public function get scale() : Number
      {
         return 0;
      }
      
      public function set scale(nScale:Number) : void
      {
      }
      
      public function set widthNoCache(nW:Number) : void
      {
      }
      
      public function set heightNoCache(nH:Number) : void
      {
      }
      
      public function get contentWidth() : Number
      {
         return 0;
      }
      
      public function get contentHeight() : Number
      {
         return 0;
      }
      
      public function set xNoCache(value:Number) : void
      {
      }
      
      public function set yNoCache(value:Number) : void
      {
      }
      
      public function get anchorY() : Number
      {
         return 0;
      }
      
      public function get anchorX() : Number
      {
         return 0;
      }
      
      public function set bgColor(nColor:*) : void
      {
      }
      
      public function get bgColor() : *
      {
         return null;
      }
      
      public function set bgAlpha(nAlpha:Number) : void
      {
      }
      
      public function get bgAlpha() : Number
      {
         return 0;
      }
      
      public function set bgMargin(nMargin:Number) : void
      {
      }
      
      public function get bgMargin() : Number
      {
         return 0;
      }
      
      public function set borderColor(color:int) : void
      {
      }
      
      public function get borderColor() : int
      {
         return 0;
      }
      
      public function set bgCornerRadius(value:uint) : void
      {
      }
      
      public function get bgCornerRadius() : uint
      {
         return 0;
      }
      
      public function set luminosity(nColor:Number) : void
      {
      }
      
      public function get luminosity() : Number
      {
         return 0;
      }
      
      public function set linkedTo(sUiComponent:String) : void
      {
      }
      
      public function get linkedTo() : String
      {
         return null;
      }
      
      public function set shadowColor(nColor:int) : void
      {
      }
      
      public function get shadowColor() : int
      {
         return 0;
      }
      
      public function get topParent() : DisplayObject
      {
         return null;
      }
      
      public function getStrata(nStrata:int) : Sprite
      {
         return null;
      }
      
      public function set dynamicPosition(bool:Boolean) : void
      {
      }
      
      public function get dynamicPosition() : Boolean
      {
         return false;
      }
      
      public function set disabled(bool:Boolean) : void
      {
      }
      
      public function set disabledExceptHandCursor(bool:Boolean) : void
      {
      }
      
      public function get disabled() : Boolean
      {
         return false;
      }
      
      public function set softDisabled(bool:Boolean) : void
      {
      }
      
      public function get softDisabled() : Boolean
      {
         return false;
      }
      
      public function set greyedOut(bool:Boolean) : void
      {
      }
      
      public function get greyedOut() : Boolean
      {
         return false;
      }
      
      public function set handCursor(bValue:Boolean) : void
      {
      }
      
      public function get handCursor() : Boolean
      {
         return false;
      }
      
      public function getStageRect() : Rectangle
      {
         return null;
      }
      
      public function remove() : void
      {
      }
      
      public function addContent(child:GraphicContainer, index:int = -1) : GraphicContainer
      {
         return null;
      }
      
      public function removeFromParent() : void
      {
      }
      
      public function getParent() : GraphicContainer
      {
         return null;
      }
      
      public function getUi() : UiRootContainer
      {
         return null;
      }
      
      public function setUi(ui:UiRootContainer, key:Object) : void
      {
      }
      
      public function getTopParent(d:DisplayObject) : DisplayObject
      {
         return null;
      }
      
      public function slide(endX:int, endY:int, time:int, inAndOut:Boolean = false, bezierX:int = 0, bezierY:int = 0) : void
      {
      }
      
      public function alphaFade(endValue:Number, time:int) : void
      {
      }
      
      public function colorSlide(endValue:Number, time:int, yoyo:Boolean) : void
      {
      }
      
      public function getPosSKey() : String
      {
         return null;
      }
      
      public function getSizeSKey() : String
      {
         return null;
      }
      
      public function getSavedPosition() : Point
      {
         return null;
      }
      
      public function setSavedPosition(x:Number, y:Number) : void
      {
      }
      
      public function getSavedDimension() : Point
      {
         return null;
      }
      
      public function setSavedDimension(x:Number, y:Number) : void
      {
      }
      
      public function resetSavedInformations(deleteInfo:Boolean = true) : void
      {
      }
      
      public function free() : void
      {
      }
   }
}

