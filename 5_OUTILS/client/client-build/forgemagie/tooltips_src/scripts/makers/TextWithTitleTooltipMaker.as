package makers
{
   import blocks.TextTooltipBlock;
   import blocks.TitleTooltipBlock;
   
   public class TextWithTitleTooltipMaker
   {
      
      private const chunkType:String = "htmlChunks";
      
      public function TextWithTitleTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip(this.chunkType + "/base/baseWithBackground.txt",this.chunkType + "/base/container.txt");
         tooltip.chunkType = this.chunkType;
         tooltip.addBlock(new TitleTooltipBlock(data.title).block);
         tooltip.addBlock(new TextTooltipBlock(data.text,null,this.chunkType).block);
         return tooltip;
      }
   }
}

