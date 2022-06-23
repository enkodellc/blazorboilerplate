Source Generators
=================

To avoid repetitive tasks, I created some `Generators`_ based on `Source Generators`_.

Notes
^^^^^
Visual Studio 16 has some issues with Source Generators, especially when you clone BlazorBoilerplate and build the first time.

Even rebuilding **BlazorBoilerplate.Shared** sometimes a lot of errors are listed, but they are fake.
Ignore them and run **BlazorBoilerplate.Server** or close and reopen Visual Studio.

Source generated is not visually updated.

.. image:: /images/entity-generator.png
   :align: center

Sometimes entities generated and present in the figure do not respect the most updated version of the source.

.. _Generators: https://github.com/enkodellc/blazorboilerplate/tree/master/src/Utils/BlazorBoilerplate.SourceGenerator/
.. _Source Generators: https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/