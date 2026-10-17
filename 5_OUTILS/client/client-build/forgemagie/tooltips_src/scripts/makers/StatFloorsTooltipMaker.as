package makers
{
   import blocks.StatFloorTooltipBlock;
   
   public class StatFloorsTooltipMaker
   {
      
      public function StatFloorsTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(pData:*, param:Object) : Object
      {
         var numFloors:uint;
         var i:int;
         var titleBlock:Object = null;
         var titleContent:String = null;
         var floor:* = undefined;
         var tooltip:Object = Api.tooltip.createTooltip("chunks/stats/baseWithBackground.txt","chunks/base/container.txt","chunks/base/separator.txt");
         titleBlock = Api.tooltip.createTooltipBlock(function():void
         {
            titleContent = titleBlock.getChunk("content").processContent({"statName":pData.statName});
         },function():String
         {
            return titleContent;
         });
         titleBlock.initChunk([Api.tooltip.createChunkData("content","chunks/stats/statFloorsTitle.txt")]);
         tooltip.addBlock(titleBlock);
         numFloors = uint(pData.floors.length);
         for(i = 0; i < numFloors; i++)
         {
            floor = pData.floors[i];
            tooltip.addBlock(new StatFloorTooltipBlock(i + 1,i + 1 == numFloors ? Api.ui.getText("ui.stats.floorStatMore",pData.floors[i][0]) : Api.ui.getText("ui.stats.floorStatInterval",pData.floors[i][0],pData.floors[i + 1][0]),Api.ui.processText(Api.ui.getText("ui.common.pointsWithValue",pData.floors[i][1]),"",pData.floors[i][1] == 1)).block);
         }
         return tooltip;
      }
   }
}

