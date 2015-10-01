# DotNet.Config

## About 

DotNet.Config is a small but powerful configuration library for .NET. 

## Features

* Easy to use: add a reference, create a new config.properties file, and with one line of code your class already has access to config settings.
* Simple name/value pair configuration in a text file, making it easy for non-technical users to update settings
* Config files that can be loaded by any component, whether the main .exe or a dll. No more fighting with .NET if you want to ship a dll with its own configuration.
* Variable substition in your config files. 
* Full support of multi-line configuration settings
* A "glue on" system applies configuration settings directly onto your class, including casting of int,enum,datetime.

## Usage

You can start using DotNet.Config with one line of code:

-- config.properties --
size=12
dateTime=1/12/2014
name=Terry Tester
color=Blue
quote=Today is $dateTime and the sky is
    bright $color.

-- /config.properties --

-- MyClass.cs --
public class MyClass {

  public enum Color { Red, Blue, Green };

  #region Glue-on properties
  //DotNet.Config will glue values from your config.properties directly onto your member variables:
  private int size; //casts non-string values 
  private DateTime dateTime;
  private string name;
  private string _quote; //supports private variables prefixed with an underscore
  private Color color; //supports enums
  #endregion

  public MyClass() {

    //this tells DotNet.Config to fetch the config.properties file and glue it onto this class
    AppSettings.GlueOnto(this);

    /*
    this.size == 12
    this.name == "Terry Tester"
    this.color == Color.Blue
    etc
    */

  }

}
-- /MyClass.cs --


## History 


