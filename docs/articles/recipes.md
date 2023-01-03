---
uid: recipes
title: "Recipes"
---

Recipes
=======

This section contains a collection of recipes for working with ZeroMQ in Bonsai. It goes beyond the basic patterns discussed earlier in the manual, and touches on problems of interfacing ZeroMQ with other packages, which might come up when you actually try to use the library for specific applications.

## Video streams

The [Vision](xref:Bonsai.Vision) package provides the [EncodeImage](xref:Bonsai.Vision.EncodeImage) and [DecodeImage](xref:Bonsai.Vision.DecodeImage) operators to compress raw image frames into byte streams using a variety of common compression algorithms. The example below shows how to create a simple video streaming server using the @pub-sub pattern.

:::workflow
![Convert images](~/workflows/convert-image.bonsai)
:::

## Binary data

ZeroMQ can be used as a transport protocol for other binary-coded data representations. The example below shows how to use the [Format](xref:Bonsai.Osc.Format) and [Parse](xref:Bonsai.Osc.Parse) operators in the [OSC](xref:Bonsai.Osc) package to transmit complex types over byte streams, while using ZeroMQ to allow more flexible networking patterns.

:::workflow
![Convert binary stream](~/workflows/convert-osc.bonsai)
:::
