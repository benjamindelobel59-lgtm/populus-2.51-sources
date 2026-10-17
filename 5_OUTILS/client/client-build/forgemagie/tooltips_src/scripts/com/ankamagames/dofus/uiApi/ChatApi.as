package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.internalDatacenter.communication.BasicChatSentence;
   import com.ankamagames.dofus.internalDatacenter.communication.ChatSentenceWithRecipient;
   import com.ankamagames.dofus.internalDatacenter.communication.ChatSentenceWithSource;
   import com.ankamagames.dofus.internalDatacenter.items.ItemWrapper;
   
   public class ChatApi
   {
      
      public function ChatApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getChannelsId() : Array
      {
         return null;
      }
      
      public function getDisallowedChannelsId() : Array
      {
         return null;
      }
      
      public function getLangIdForCommunityChannel() : int
      {
         return 0;
      }
      
      public function getChatColors() : Array
      {
         return null;
      }
      
      public function getSmileyMood() : int
      {
         return 0;
      }
      
      public function getMessagesByChannel(channel:uint) : Array
      {
         return null;
      }
      
      public function getParagraphByChannel(channel:uint) : Array
      {
         return null;
      }
      
      public function getHistoryMessagesByChannel(channel:uint) : Array
      {
         return null;
      }
      
      public function getMessagesStoredMax() : uint
      {
         return 0;
      }
      
      public function addParagraphToHistory(id:int, p:Object) : void
      {
      }
      
      public function removeLinesFromHistory(value:int, channel:int) : void
      {
      }
      
      public function setMaxMessagesStored(val:int) : void
      {
      }
      
      public function getMaxMessagesStored() : int
      {
         return 0;
      }
      
      public function newBasicChatSentence(id:uint, msg:String, channel:uint = 0, time:Number = 0, finger:String = "") : BasicChatSentence
      {
         return null;
      }
      
      public function newChatSentenceWithSource(id:uint, msg:String, channel:uint = 0, time:Number = 0, finger:String = "", senderId:Number = 0, prefix:String = "", senderName:String = "", objects:Object = null) : ChatSentenceWithSource
      {
         return null;
      }
      
      public function newChatSentenceWithRecipient(id:uint, msg:String, channel:uint = 0, time:Number = 0, finger:String = "", senderId:Number = 0, prefix:String = "", senderName:String = "", receiverName:String = "", receiverId:Number = 0, objects:Object = null) : ChatSentenceWithRecipient
      {
         return null;
      }
      
      public function getTypeOfChatSentence(msg:Object) : String
      {
         return null;
      }
      
      public function searchChannel(chan:String) : int
      {
         return 0;
      }
      
      public function getHistoryByIndex(name:String, index:uint) : String
      {
         return null;
      }
      
      public function getRedChannelId() : uint
      {
         return 0;
      }
      
      public function getStaticHyperlink(string:String) : String
      {
         return null;
      }
      
      public function newChatItem(item:ItemWrapper) : String
      {
         return null;
      }
      
      public function addAutocompletionNameEntry(name:String, priority:int) : void
      {
      }
      
      public function getAutocompletion(subString:String, count:int) : String
      {
         return null;
      }
      
      public function getGuildLink(pGuild:*, pText:String = null) : String
      {
         return null;
      }
      
      public function getAllianceLink(pAlliance:*, pText:String = null, pLinkColor:String = null, pHoverColor:String = null) : String
      {
         return null;
      }
      
      public function changeCssHandler(val:String) : void
      {
      }
      
      public function logChat(text:String, cssClass:String) : void
      {
      }
      
      public function launchExternalChat() : void
      {
      }
      
      public function clearConsoleChat() : void
      {
      }
      
      public function isExternalChatOpened() : Boolean
      {
         return false;
      }
      
      public function setExternalChatChannels(pChannels:Array) : void
      {
      }
      
      public function addHtmlLink(pText:String, pHref:String) : String
      {
         return null;
      }
      
      public function addSpan(pText:String, pStyle:Object) : void
      {
      }
      
      public function escapeChatString(inStr:String) : String
      {
         return null;
      }
      
      public function unEscapeChatString(inStr:String) : String
      {
         return null;
      }
   }
}

