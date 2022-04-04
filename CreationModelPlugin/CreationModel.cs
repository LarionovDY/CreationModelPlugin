using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;   //получение доступа к элементу

            ////поиск всех стен в модели
            //var res1 = new FilteredElementCollector(doc)    
            //    .OfClass(typeof(Wall))  //быстрый фильтр
            //    //.Cast<Wall>() //вызывает исключение при попадании типа который нельзя преобразовать (лучше не использовать)
            //    .OfType<Wall>() //выполняет фильтрацию по типу (медленный тип)
            //    .ToList();

            ////поиск всех типов стен стен в модели
            //var res2 = new FilteredElementCollector(doc)    
            //    .OfClass(typeof(WallType))  //быстрый фильтр                                     
            //    .OfType<WallType>() //выполняет фильтрацию по типу (медленный тип)
            //    .ToList();

            ////поиск всех загружаемых семейств в модели
            //var res3 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilyInstance))  //быстрый фильтр                                     
            //    .OfCategory(BuiltInCategory.OST_Doors)
            //    .OfType<FamilyInstance>() //выполняет фильтрацию по типу (медленный тип)
            //    .ToList();

            ////поиск всех загружаемых семейств по имени
            //var res4 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilyInstance))  //быстрый фильтр                                     
            //    .OfCategory(BuiltInCategory.OST_Doors)
            //    .OfType<FamilyInstance>() //выполняет фильтрацию по типу (медленный тип)
            //    .Where(x => x.Name.Equals("0915 x 2134 мм"))
            //    .ToList();

            ////поиск всех экземпляров загружаемых семейств
            //var res5 = new FilteredElementCollector(doc)
            //    .WhereElementIsNotElementType()             
            //    .ToList();

            //получение уровней из модели
            Level level1 = GetLevel(doc, "Уровень 1");
            Level level2 = GetLevel(doc, "Уровень 2");

            //задание геометрических размеров здания
            double width = 10000;
            double depth = 5000; 

            //создание списка стен
            List<Wall> walls = CreateWalls(doc, width, depth, level1, level2);

            return Result.Succeeded;
        }

        public List<Wall> CreateWalls(Document doc, double _width, double _depth, Level level, Level upperLevel)    //создание стен по прямоугольному периметру, по введенным размерам в миллиметрах, высота стен определяется привязкой к верхнему уровню
        {
            List<Wall> walls = new List<Wall>();    //создание массива стен

            if (_width > 0 && _depth > 0 && level!=null && upperLevel != null)
            {
                double width = UnitUtils.ConvertToInternalUnits(_width, UnitTypeId.Millimeters); //задание ширины здания в миллиметрах
                double depth = UnitUtils.ConvertToInternalUnits(_depth, UnitTypeId.Millimeters); //задание глубины здания в миллиметрах

                double dx = width / 2;
                double dy = depth / 2;

                List<XYZ> points = new List<XYZ>(); //создание массива с координатами концов стен
                points.Add(new XYZ(-dx, -dy, 0));
                points.Add(new XYZ(dx, -dy, 0));
                points.Add(new XYZ(dx, dy, 0));
                points.Add(new XYZ(-dx, dy, 0));
                points.Add(new XYZ(-dx, -dy, 0));                

                Transaction transaction = new Transaction(doc, "Построение стен"); //транзакция в которой будем создавать стены
                transaction.Start();
                for (int i = 0; i < 4; i++)
                {
                    Line line = Line.CreateBound(points[i], points[i + 1]); //создание отрезка проекции стены
                    Wall wall = Wall.Create(doc, line, level.Id, false);   //создание стены
                    walls.Add(wall);    //добавление созданной стены в массив
                    wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(upperLevel.Id);   //привязка высоты стены к уровню 2
                }
                transaction.Commit();
            }
            return walls;
        }

        public Level GetLevel(Document doc, string levelName)   //получение уровня из модели по имени
        {
            List<Level> listLevel = new FilteredElementCollector(doc)    //создание списка всех уровней в модели
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();
            if (listLevel != null)
            {
                Level level = listLevel    //поиск уровня 
                .Where(x => x.Name.Equals(levelName))
                .FirstOrDefault();
                if (level != null)
                    return level;
            }
            return null;
        }
    }
}
