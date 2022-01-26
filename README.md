## Составитель расписания
Программа принимает на вход 3 таблицы (учебный план, требования преподавателей, аудитории) и выдает сгенерированное расписание
[пример](https://docs.google.com/spreadsheets/d/1JJm-ZBoHfumv82gRpGCdF_XZBPi_wpptUaHooGj3fBU)

Общая схема:
Распарсив требования, программа создает первичные meeting-и (пары). У них еще нет конкретного времени, аудитории и группы.
Далее по первичному meeting-у можно сгенерировать кучу вторичных с конкретными группами, местом и временем проведения. На каждом шаге алгоритма выбирается одна вторичная пара, которую надо реализовать (ну т.е. поставить в расписание).
Разные алгоритмы по разному выбирают какую вторичную пару реализовать (но везде так или иначе участвуют estimator-ы)
Эстиматоры - это маленькие классы - оценщики расписания. С их помощью определяется качество расписания (причем можно оценивать и не до конца составленное расписание)
Примеры эстиматоров: количество окон, количество пар в день (не должно слишком или слишком мало), количество смен локаций (студентам не хочется мотаться по всему городу)
