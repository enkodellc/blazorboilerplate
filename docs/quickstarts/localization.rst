Localization
============
Localization has moved from Resource based localization to Database based localization,
so translation are no more in dll satellite libraries, but in table **LocalizationRecords**.
Look at `LocalizationDbContext <https://github.com/enkodellc/blazorboilerplate/blob/master/src/Server/BlazorBoilerplate.Storage/LocalizationDbContext.cs>`_.

The localization code is in `BlazorBoilerplate.Shared project <https://github.com/enkodellc/blazorboilerplate/tree/master/src/Shared/BlazorBoilerplate.Shared/SqlLocalizer>`_.

At this time **Data Annotations** do not support **IStringLocalizer<>**,
so to localize validation error messages, we have to use `Blazored.FluentValidation <https://github.com/Blazored/FluentValidation>`_.


The **Localization project** contains main application **Strings.[language-country].resx** files with available languages.

.. note:: This project is only used to store resx files to init **LocalizationRecords** table in `DatabaseInitializer <https://github.com/enkodellc/blazorboilerplate/blob/master/src/Server/BlazorBoilerplate.Storage/DatabaseInitializer.cs#L87>`_.
   Warning! This is not a production ready solution and it will change in future.


If you add new keys, you have to provide always english (en-us) neutral language and only other languages you know.

Managing all these resx files can be tricky, so it is better to install a Visual Studio extension like `ResXManager`_.

.. image:: /images/resx-resource-manager.png
   :align: center

With `ResXManager`_ is easy to add a new language from the user interface.

Then you have to add the new supported culture in `Settings.cs <https://github.com/enkodellc/blazorboilerplate/blob/master/src/Shared/BlazorBoilerplate.Shared/SqlLocalizer/Settings.cs>`_.

For module specific localization, consider using a local resx file.

Everywhere you need localized text, inject **IStringLocalizer<Global>** and look how it is used throughout the code.

::

 L["text key usually in english"]




.. _ResXManager: https://marketplace.visualstudio.com/items?itemName=TomEnglert.ResXManager