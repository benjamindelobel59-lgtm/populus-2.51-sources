package com.ankamagames.berilia.components
{
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   
   public class ProgressBar extends GraphicContainer
   {
      
      public function ProgressBar()
      {
         super();
      }
      
      public function get snapToSeparators() : Boolean
      {
         return new Boolean();
      }
      
      public function set snapToSeparators(v:Boolean) : void
      {
      }
      
      public function set value(v:Number) : void
      {
      }
      
      public function get value() : Number
      {
         return 0;
      }
      
      public function set innerValue(v:Number) : void
      {
      }
      
      public function get innerValue() : Number
      {
         return 0;
      }
      
      public function get barPadding() : int
      {
         return 0;
      }
      
      public function set barPadding(value:int) : void
      {
      }
      
      public function set barColor(v:*) : void
      {
      }
      
      public function set innerBarColor(v:*) : void
      {
      }
      
      public function set innerBarAlpha(v:Number) : void
      {
      }
      
      public function set separators(numberOfSeparator:uint) : void
      {
      }
      
      public function set clickable(v:Boolean) : void
      {
      }
      
      public function set barForegroundThemeData(themeDataId:String) : void
      {
      }
      
      public function set barBackgroundThemeData(themeDataId:String) : void
      {
      }
   }
}

