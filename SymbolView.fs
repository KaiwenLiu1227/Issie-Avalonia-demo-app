﻿namespace Selector.mvu

open Selector.mvu.DisplayModelTypes
open Selector.mvu.Symbol

module SymbolView =
    
    open System
    open Avalonia
    open Avalonia.Controls
    open Avalonia.Controls.Shapes
    open Avalonia.FuncUI
    open Avalonia.FuncUI.DSL
    open Avalonia.Media
    open Avalonia.FuncUI.Types
    open SheetModel
    open SymbolHelper
    open DisplayModelTypes
    
    let drawComponent polygonParameter dispatch=
            let points = genPoints polygonParameter.compType 20 20
            let pointList = points |> Array.toList |> List.map (fun p -> Point(p.X, p.Y))
            Polygon.create [
                Polygon.points pointList
                Polygon.stroke (SolidColorBrush(polygonParameter.Stroke))
                Polygon.strokeThickness polygonParameter.StrokeThickness
                Polygon.fill (SolidColorBrush(polygonParameter.Fill, polygonParameter.FillOpacity))
                Component.onPointerPressed (fun args -> dispatch (OnPress polygonParameter.Id))
            ]
    
    // WITH COMPONENT KEY BIND FOR CACHING 
    let renderSymbol (initialProps: PolygonParameters) dispatch : Component=
        Component (fun ctx ->
            let props = ctx.useState initialProps
            printfn "polygon %d render called" props.Current.Id
            let xPosition = getCompPos props.Current "X"
            let yPosition = getCompPos props.Current "Y"
            // printfn "x %f y %f" xPosition yPosition
            printfn "%d render called" initialProps.renderCnt

            ctx.attrs[
                Component.renderTransform (
                    TranslateTransform(xPosition, yPosition)
                )
            ]
            drawComponent props.Current dispatch
        )
    
     (*// STANDARD IMPLEMENTATION WITHOUT COMPONENT KEY BIND FOR CACHING 
    let renderSymbol props dispatch :IView=
        printfn "polygon %d render called" props.Id
        let xPosition = getCompPos props "X"
        let yPosition = getCompPos props "Y"
        Viewbox.create[
            Viewbox.renderTransform (
                TranslateTransform(xPosition, yPosition)
            )
            Viewbox.child (
               drawComponent props dispatch 
            )
        ]   *) 


    let symbolView state dispatch =
            let handlePointerMoved (args: Input.PointerEventArgs) =
                    let newPos = args.GetPosition(null) 
                    let roundUpToTen n = Math.Ceiling(n / 10.0) * 10.0
                    // Apply the function to both X and Y coordinates
                    let roundedX = roundUpToTen newPos.X
                    let roundedY = roundUpToTen newPos.Y
                    // Dispatch the Move message with the rounded coordinates
                    dispatch (Move { X = roundedX; Y = roundedY })
                
            let matrixTransform = MatrixTransform(Matrix.CreateRotation(state.rotation))

            Canvas.create [
                Canvas.renderTransform matrixTransform
                Canvas.height 250
                Canvas.width 250
                Canvas.onPointerWheelChanged (fun args -> dispatch (Rotate args))
                Canvas.onPointerMoved (fun args -> handlePointerMoved args)
                Canvas.onPointerReleased (fun args -> dispatch OnRelease) 
                Canvas.children (
                    state.polygonParameters
                    |> Array.map (fun param ->
                        ContentControl.create[
                            ContentControl.content (
                                renderSymbol param dispatch
                            )
                        ] :> IView
                    )
                    |> Array.toList
                )
            ]    
        

