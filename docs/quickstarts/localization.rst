Localization
============
**Localization project** contains main application **Strings.[language-country].resx** files with available languages.

If you add new keys, you have to provide always english (en-us) neutral language and only other languages you know.

Managing all these resx files can be tricky, so it is better to install a Visual Studio extension like `ResXManager`_.

.. image:: /images/resx-resource-manager.png
   :align: center

With `ResXManager`_ is easy to add a new language from the user interface.

Then you have to add the new supported culture in **Settings.cs** of **Localization project**.

For module specific localization, consider using a local resx file.

Everywhere you need localized text, inject **IStringLocalizer<Strings>** and look how it is used throughout the code.

::

 L["text key usually in english"]




.. _ResXManager: https://marketplace.visualstudio.com/items?itemName=TomEnglert.ResXManager