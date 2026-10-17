package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.berilia.types.graphic.UiRootContainer;
   import com.ankamagames.dofus.internalDatacenter.world.WorldPointWrapper;
   
   public class UiTutoApi
   {
      
      public function UiTutoApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getSubhintInspector() : Object
      {
         return null;
      }
      
      public function getSubhints() : void
      {
      }
      
      public function updateSubhint(data:Object) : void
      {
      }
      
      public function moveSubhint(data:Object) : void
      {
      }
      
      public function createSubhint(data:Object) : void
      {
      }
      
      public function deleteSubhint(data:Object) : void
      {
      }
      
      public function previewSubhint(data:Object) : void
      {
      }
      
      public function closeSubhintPreview() : void
      {
      }
      
      public function isInFight() : Boolean
      {
         return false;
      }
      
      public function isInPreFight() : Boolean
      {
         return false;
      }
      
      public function isSpectator() : Boolean
      {
         return false;
      }
      
      public function isInTutorialArea() : Boolean
      {
         return false;
      }
      
      public function currentMap() : WorldPointWrapper
      {
         return null;
      }
      
      public function getCurrentTarget() : UiRootContainer
      {
         return null;
      }
      
      public function getSubhintDataArrayWithParentUID(parentUID:String) : Array
      {
         return null;
      }
      
      public function getSubhintData() : Object
      {
         return null;
      }
      
      public function guidedActivated() : Boolean
      {
         return false;
      }
      
      public function canDisplayHelp(uirc:UiRootContainer = null) : Boolean
      {
         return false;
      }
      
      public function subHintsAreDisplayed(target:UiRootContainer) : Boolean
      {
         return false;
      }
      
      public function uiTutoTabLaunch() : void
      {
      }
      
      public function showSubhintOnUiLoaded(uiRoot:UiRootContainer) : void
      {
      }
      
      public function showSubHints(tabDisplayed:String = "", onClick:Boolean = true) : void
      {
      }
      
      public function mergeData(uiRoot:UiRootContainer, guided:Boolean, tab:String = "", onClick:Boolean = true) : Boolean
      {
         return false;
      }
      
      public function showNextSubHint(order:int) : void
      {
      }
      
      public function skipTuto() : void
      {
      }
      
      public function closeSubHints() : void
      {
      }
      
      public function closeEndTooltip() : void
      {
      }
      
      public function getGuidedAlreadyDone(tabDisplayed:String = "") : Boolean
      {
         return false;
      }
      
      public function getSpecificGuidedAlreadyDone(tabDisplayed:String = "") : Boolean
      {
         return false;
      }
      
      public function sameParentContainer(target:GraphicContainer, currentTarget:GraphicContainer) : Boolean
      {
         return false;
      }
      
      public function highLightSubhint(anchoredElementName:String, value:Boolean) : void
      {
      }
      
      public function getSubHintlist() : Array
      {
         return null;
      }
      
      public function getNumberOfSubHints() : int
      {
         return 0;
      }
      
      public function displayTutoOfSelectedTab(container:GraphicContainer, target:Object) : void
      {
      }
      
      public function resetGuidedUiHints() : void
      {
      }
      
      public function getUiClass(uiClassName:String) : UiRootContainer
      {
         return null;
      }
      
      public function getAllOpenedUiWithSubHints() : Array
      {
         return null;
      }
   }
}

