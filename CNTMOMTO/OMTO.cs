
namespace CNTMOMTO
{

using System;
using System.Windows.Forms;

using Intermech;
using Intermech.Interfaces;
using Intermech.Interfaces.Client;
using Intermech.Interfaces.Plugins;
using Intermech.Navigator.ContextMenu;
using Intermech.Navigator.Interfaces;
using Intermech.DataFormats;
using Intermech.Interfaces.Projects;
using Intermech.Interfaces.Workflow;

  


    public class PluginPackage : AbstractPackage
    {
        public PluginPackage()
            : base("Закупки")
        {
        }

        internal static IServiceProvider _serviceProvider;
       // internal static dynamic com1C;

        public override void Load(IServiceProvider serviceProvider)
        {
            // Сохраняем ссылку на контейнер сервисов
            _serviceProvider = serviceProvider;

         /*   COMConnector cntr = new COMConnector();
            string conStr = @"Srvr=""172.16.243.20:1641""; Ref=""peo_2""; Usr=""IPS Техархив""; Pwd=""9875321"";";
            com1C = cntr.Connect(conStr);
            */
            // Пример №6: Команда контекстного меню
            CreateCommand();

            base.Load(serviceProvider);
        }

        public override void Unload()
        {
            base.Unload();
        }

        internal void CreateCommand()
        {
            // Получаем ссылку на службу по регистрации расширений "Навигатора"
            IFactory factory = ServicesManager.GetService(typeof(IFactory)) as IFactory;

            // Получаем ссылку на сервис именованных значков
            INamedImageList _images = _serviceProvider.GetService(typeof(INamedImageList)) as INamedImageList;

            factory.ContextMenuTemplate.Nodes.Add(new MenuTemplateNode("LoadGPZ",
                            "Сформировать заявку на включение в ГПЗ",
                            _images.ImageIndex("imgSamples.CopyObject"),
                            20,
                            Int32.MaxValue));

            // Регистрируем провайдер команд контекстного меню для категории "Версии объектов"
            factory.AddCommandsProvider(Consts.CategoryObjectVersion, new LoadFrom1CCommandProvider());
        }


    }

    internal class LoadFrom1CCommandProvider : ICommandsProvider
    {

        //internal static dynamic com1C;

        /* public LoadFrom1CCommandProvider(dynamic comObj)
         {
             com1C = comObj;
         }*/

        public CommandsInfo GetMergedCommands(ISelectedItems items, IServiceProvider viewServices)
        {
            // ВНИМАНИЕ! Основное требование к данному методу – нельзя выполнять обращения к базе даных 
            // для того, чтобы проверить, можно ли отображать команду меню или нет!

            // Список добавленных или перекрытых команд контекстного меню
            CommandsInfo commandsInfo = new CommandsInfo();

            // Есть один выделенный элемент
            if (items != null && items.Count == 1)
            {
                // Пробуем получить описание выделенного объекта
                IDBTypedObjectID objID = items.GetItemData(0, typeof(IDBTypedObjectID)) as IDBTypedObjectID;
                //   MessageBox.Show(objID.Caption);
                // Выделен объект
                if (objID != null)
                {
                    SessionKeeper keeper = new SessionKeeper();
                    IDBObjectType objectType = keeper.Session.GetObjectType(objID.ObjectType);
                    /*  while (objectType.ParentTypeID > 0)
                      {
                          objectType = keeper.Session.GetObjectType(objectType.ParentTypeID);
                      }
                      */
                    // Можем добавить команду 
                    if (objectType.ObjectTypeName == "Поручение на закупку")
                    {
                        commandsInfo.Add("LoadGPZ",
                       new CommandInfo(TriggerPriority.ItemCategory,
                       new ClickEventHandler(LoadFrom1CCommandProvider.CommandClick)));
                        
                    }
                  

                }

            }

            // Вернём список
            return commandsInfo;
        }

        public CommandsInfo GetGroupCommands(ISelectedItems items, IServiceProvider viewServices)
        {
            // ВНИМАНИЕ! Основное требование к данному методу – нельзя выполнять обращения к базе даных 
            // для того, чтобы проверить, можно ли отображать команду меню или нет!

            // Список добавленных или перекрытых команд контекстного меню
            CommandsInfo commandsInfo = new CommandsInfo();

            // Вернём список
            return commandsInfo;
        }

        internal static void CommandClick(ISelectedItems items, IServiceProvider viewServices, object additionalInfo)
        {
            
            // Пробуем получить описание выделенного объекта
            IDBTypedObjectID objID = items.GetItemData(0, typeof(IDBTypedObjectID)) as IDBTypedObjectID;
            using (SessionKeeper keeper = new SessionKeeper())
            {
                // Выделен объект
                if (objID == null) return;
                // SessionKeeper keeper = new SessionKeeper();
                NumByWords numwords = new NumByWords();

                IDBRelationCollection relationCollection = keeper.Session.GetRelationCollection(MetaDataHelper.GetRelationTypeID(new Guid("cadd927b-306c-11d8-b4e9-00304f19f545")));
                IDBObject objZZ = keeper.Session.GetObject(objID.ObjectID);
                if (objZZ.LCStep != 10035)
                {
                    MessageBox.Show("Невозможно сформировать ГПЗ, так как закупочная заявка не подписана");
                    return;
                }

                if (objZZ.ProjectID != 0)
                {
                    IDBProjectObject objProj = keeper.Session.GetObject(objZZ.ProjectID) as IDBProjectObject;
                    

                   // if (objProj != null)
                    //{
                    //Проверка участник ли пользователь или нет.
                        if (objProj.IsProjectManager(keeper.Session.UserID) || objProj.IsProjectParticipant(keeper.Session.UserID))
                        {
                        }
                        else
                        {
                            MessageBox.Show("Вы не являетесь учасником проекта! Создание невозможно" );
                            return;
                        }
                    //}
                }



                // Получаем у системного контейнера сервисов ссылку на службу по созданию новых объектов
                IObjectCreatorService creatorService = ServicesManager.GetService(typeof(IObjectCreatorService)) as IObjectCreatorService;
               
                //IObjectsChangingAnalyzer analyzerService = ServicesManager.GetService(typeof(IObjectsChangingAnalyzer)) as IObjectsChangingAnalyzer;
                // analyzerService.Analyze(keeper.Session,null);
                // Вызываем мастер по созданию новых объектов, получаем идентификатор версии нового объекта
                Int64 newObjectID = creatorService.CreateObjectByTypeDialog(1998);
                
                // Если объект не был создан - выход
                if (newObjectID == Intermech.Consts.UnknownObjectId || newObjectID == -1) return;
         
               // analyzerService.Analyze(keeper.Session, co);
                relationCollection.Create(objID.ObjectID, newObjectID);
                // Получаем ссылку на службу ведомлений
                INotificationService svc = ServicesManager.GetService(typeof(INotificationService)) as INotificationService;
                // Проверим её наличие
                if (svc == null) return;                
                DBObjectsEventArgs oe = new DBObjectsEventArgs(NotificationEventNames.ObjectsCreated, newObjectID);
                IDBObject obj = keeper.Session.GetObject(newObjectID);
                if (objZZ.ProjectID.ToString() != null || objZZ.ProjectID.ToString() != String.Empty)
                    obj.ProjectID = objZZ.ProjectID;
                              
                if (obj != null)
                {

                    obj.GetAttributeByName("Ссылка на заявку").Value = objZZ.ObjectID;
                }
                else
                {
                    MessageBox.Show("Случилось невероятное");
                }
                
              
                string[] decnum = obj.GetAttributeByName("Сумма ГПЗ").AsString.Split(',');
                
                if (decnum[1].ToString() == String.Empty)
                {
                    obj.GetAttributeByName("Сумма ГПЗ").AsString += "00";

                }
               
                decimal convertsum = Convert.ToDecimal(obj.GetAttributeByName("Сумма ГПЗ").AsString.Replace(" ","0"));
                decimal nds = (convertsum/100)*20;
                obj.GetAttributeByName("Сумма ТКП форматированная").Value = numwords.RurPhrase(convertsum).ToString() + "), НДС 20% " + nds.ToString() +"("
                    + numwords.RurPhrase(nds).ToString();

                svc.FireEvent(newObjectID, oe);


                ICaptureFileChangesService cfs = ServicesManager.GetService(typeof(ICaptureFileChangesService)) as ICaptureFileChangesService;
                cfs.CaptureChanges(newObjectID, SaveChangesMode.Checkin, null);
                    
    
                obj.CheckIn();

                Intermech.Navigator.DBObjects.Descriptor descriptor = new Intermech.Navigator.DBObjects.Descriptor(obj.ObjectID);
              
                // Пробуем открыть вновь созданный объект в новом окне "Навигатора"
                Intermech.Navigator.Utils.OpenNewWindow(descriptor, null);
           
                keeper.Dispose();
            }
             

        }


    }

}
