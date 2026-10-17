package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.PlayedCharacterApi;
   import com.ankamagames.dofus.uiApi.TooltipApi;
   import com.ankamagames.dofus.uiApi.UtilApi;
   import d2hooks.*;
   
   public class WorldRpPaddockItemTooltipUi
   {
      
      public var tooltipApi:TooltipApi;
      
      public var uiApi:UiApi;
      
      public var playerApi:PlayedCharacterApi;
      
      public var utilApi:UtilApi;
      
      public var dataApi:DataApi;
      
      public var lbl_name:Label;
      
      public var lbl_durability:Label;
      
      public var bgCtr:GraphicContainer;
      
      public function WorldRpPaddockItemTooltipUi()
      {
         super();
      }
      
      public function main(oParam:Object = null) : void
      {
         var data:Object = oParam.data;
         this.lbl_name.text = data.name;
         this.lbl_name.fullWidthAndHeight();
         this.lbl_durability.text = data.durability.durability + "/" + data.durability.durabilityMax;
         this.lbl_durability.fullWidthAndHeight();
         this.bgCtr.width = this.lbl_name.width > this.lbl_durability.width ? this.lbl_name.width + 10 : this.lbl_durability.width + 10;
         this.tooltipApi.place(oParam.position,oParam.showDirectionalArrow,oParam.point,oParam.relativePoint,oParam.offset);
      }
      
      public function unload() : void
      {
      }
   }
}

