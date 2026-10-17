package populus
{
   public class PopulusBoot
   {
      
      public function PopulusBoot()
      {
         super();
      }
      
      public static function open(param1:*) : void
      {
         var uiApi:* = param1;
         try
         {
            // a chaque entree en jeu (changement de personnage compris) : barre rechargee -> boutons remis en gris,
            // les options du serveur (One Window, IP Drop, Sasa, Passe-tour) repartent desactivees a la connexion
            if(!uiApi)
            {
               return;
            }
            if(uiApi.getUi("populusButtons"))
            {
               uiApi.unloadUi("populusButtons");
            }
            uiApi.loadUi("populusButtons","populusButtons");
         }
         catch(e:Error)
         {
         }
      }
   }
}

