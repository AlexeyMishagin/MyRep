  Дистрибутив создается автоматически при мердже разработчиком изменений в ветку Develop или Release. Если нет ошибок дистрибутив отправляется в SCCM.  
Для автоматической подготовки дистрибутива используется GitlabRunner и VisualStudio 2019.  
При мердже изменений в ветку Develop или Release происходит создание дистрибутива и его иснталляция. Если нет ошибок дистрибутив отправляется в SCCM.
