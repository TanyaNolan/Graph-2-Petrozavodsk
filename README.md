# Graph-2-Petrozavodsk
Программа для построения графа дорог города Петрозаводк на основе данных проекта OpenStreetMap.

Среда разработки: Visual Studio 2017.
Используемый язык: C#.

## Использование

Скачайте любым способом .osm файл с необходимым участком карты, назовите его "map.osm". 
(Важно!) Файл "map.osm" должен находиться в одной папке с "graphs2.exe".

Запустите файл "graphs2.exe", следуйте инструкциям.

К проекту прикреплены результаты для моего города (Петрозаводск), вводилась точка с координатами LAT=61,77 LON=34,36.

Синим кругом на карте отмечена стартовая точка, ближайшая точка отмечена красным кругом.

## Анализ тестирования

Проводится тестирование алгоритма на 100 равномерно распределенных точках.

### Анализ времени выполнения
Время выполнения оценивалось путем подсчета суммы времени выполнения программы для каждой точки.

1. Алгоритм Дейкстры: среднее время выполнения - 11,5 секунд.

2. Алгоритм Левита: среднее время выполнения - 4,9 секунды.

3. Алгоритм A*: с метрикой "Евклидово расстояние": среднее время выполнения - 1,4 секунды.

**Вывод:** Несмотря на то, что в алгоритме Дейстры применялась оптимизация(выход из алгоритма осуществлялся сразу же после достижения последней точки) алгоритм Дейкстры работает крайне долго. Алгоритм Левита работает быстрее так маршруты находядся сразу до всех 10 точек ресторанов за один запуск алгоритма. Алгоритм A* работает быстрее всего.
