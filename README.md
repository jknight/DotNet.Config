# DotNet.Config

![DotNet.Config](https://raw.githubusercontent.com/jknight/DotNet.Config/master/DotNet.Config.png "DotNet.Config")

## About 

DotNet.Config is a small but powerful configuration library for .NET originally inspired by [Java's .properties files](https://commons.apache.org/proper/commons-configuration/userguide/howto_properties.html).

This library provides a simple way to manage configuration settings in a name/value format text file. It applies configuration settings directly onto your class member variables..
It uses a transparent convention over configuration approach: if your class has a member variable that matches the name of a setting, it will automatically apply the value to your class.

* **For developers**: 'Glue' configuration settings directly onto your classes based on the convention of member variables in classes matching the names of configuration settings.
* **For your users**: Configuration settings are a breeze to edit in a simple .properties text file. No more calls over botched app/web.config files. 

## Why?

.NET already has a [configuration system](https://msdn.microsoft.com/en-us/library/system.configuration.configurationmanager.aspx), so what's the point of this? This library makes life easier and fills some gaps:
* No Xml configuration. Xml baffles non-technical users who struggle to find which settings need to be updated. In addition, settings in Xml need to be HTML encoded.
Have a setting with >, <, & in them? Someone is guaranteed to botch it. Have a long multi-line string setting? No fun with web/app.config. 
* Working with System.Configuration is tedious and verbose. DotNet.Config, but contrast, is nearly invisible and automatically applies all your settings.
* System.Configuration is a notorious headache if you want to ship a dll with its own configuration file. This is a non-issue with DotNet.Config

## Features

* **Easy to add**: add a reference, create a new config.properties file, and with one line of code your class already has access to config settings.
* **Hassle free config files**: simple and straight forward text-based config files. No more HTML encoding settings in ugly XML files. 
* **Load anywhere**: configuration settings can be loaded by any component, whether the main .exe or a dll. No more fighting with .NET if you want to ship a dll with its own configuration.
* **Inspired by Java**: config.properties files are inspired by Java's simple text-based config files 
* **Variable substition**: baked in support for variable substitution in your config files. 
* **Flexible**: Full support of multi-line configuration settings
* **Glue it on**: features a unique "glue on" approach that applies configuration settings directly onto your class. 
* **Strong Typing**: It will automatically cast to your ints, strings, dates, enums. You can also load directly into generic lists - see the example below
* **Unit Tested**: Includes a suite of unit tests for all configuration scenarios

## Usage

Start using DotNet.Config with one line of code.

1. Add a reference to DotNet.Config
2. Add "using DotNet.Config;" to your class imports
3. Create a config.properties file (properties->copy to outputdirectory) like this:

  ````dosini
  # Lines starting with a # are comments and will be ignored
  size=12
  dateTime=1/12/2014
  name=Terry Tester
  color=Blue
  templateFile=$PATH\myTemplate.xml
  quote=Today is $dateTime and the sky is
      bright $color.

  items.A=hello list
  items.B=guten tag list
  items.C=bonjour list

  numbers.X=10
  numbers.Y=20
  numbers.Z=30

  ````
4. And you're ready to go:

  ````csharp
  using DotNet.Config;
  public class MyClass {
  
    public enum Color { Red, Blue, Green };
  
    #region Glue-on properties
    //DotNet.Config will glue values from your config.properties 
    //directly onto your member variables:
    private int size; //casts non-string values 
    private DateTime dateTime;
    //Substitutes $PATH with the value of the directory this component is in.
    private string templateFile;
    private string name;
    //also supports private variables prefixed with an underscore:
    private string _quote; 
    private Color color; //supports enums
    private List<string> items; //supports lists of strings
    private List<int> numbers; //supports lists of numbers 
    #endregion
  
    public MyClass() {
  
      //tell DotNet.Config to glue config.properties onto this class:
      AppSettings.GlueOnto(this); 
  
      /* And presto, your variables are loaded from the config !
      this.size == 12
      this.name == "Terry Tester"
      this.color == Color.Blue
      
      this.items is a list of 3 strings
      this.numbers is a list of 3 integers 
      */
  
    }
  }
  ````
  

## History 

This code grew out of a need to include configuration settings for a dll.

Over time, it evolved into a light weight configuration library that simply "glues" properties onto classes.


