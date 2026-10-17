package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.TooltipApi;
   
   public class TextWithTitleTooltipUi
   {
      
      public var tooltipApi:TooltipApi;
      
      public var uiApi:UiApi;
      
      public var sysApi:SystemApi;
      
      public var lbl_content:Label;
      
      public var backgroundCtr:Object;
      
      public function TextWithTitleTooltipUi()
      {
         super();
      }
      
      public function main(oParams:Object = null) : void
      {
         this.uiApi.setLabelStyleSheet(this.lbl_content,this.sysApi.getConfigEntry("config.ui.skin") + "css/tooltip_default.css");
         this.lbl_content.multiline = true;
         this.lbl_content.text = oParams.tooltip.htmlText;
         this.lbl_content.fullWidthAndHeight(0,30);
         this.backgroundCtr.height = this.lbl_content.contentHeight;
         this.backgroundCtr.width = this.lbl_content.contentWidth;
         this.tooltipApi.place(oParams.position,oParams.showDirectionalArrow,oParams.point,oParams.relativePoint,oParams.offset);
      }
   }
}

