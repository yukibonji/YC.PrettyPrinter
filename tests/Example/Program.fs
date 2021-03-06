﻿module Example

open Expr
open Stmt
open Stmt.Parser
open CoreParser

let ast = 
    &"if (1+5+7) { write(1+2)} else{ write(1+3+4)};"
    //&"if (1+5+7) { write(longVarName+longVarName); write(longVarName+longVarName)} else{ write(longVarName+longVarName)};"
    //&"if (1+5+7) { write(1+2); write(1+2)} else{ write(1+3+4)};"
    //&"if (1+5+7) { write(1+2); write(1+2)} else{ write(veryVeryVeryVeryLongVarName+222+3+4)};"
    |> Stmt.Parser.parse () 
    |> List.head
    |> fst

let width = 25

module SF =
    open Microsoft.FSharp.Text.StructuredFormat
    open Microsoft.FSharp.Text.StructuredFormat.LayoutOps

    let rec print ast =
        match ast with
        | Expr.BinOp(x, y, z) -> print y ++ wordL (string x) ^^ print z
        | Expr.Num n -> string n |> wordL
        | Expr.Var n -> string n |> wordL
    and printS ast =
        match ast with
        | Stmt.Write s -> wordL "write" ^^ bracketL(print s)
        | Stmt.Seq(s1, s2) -> (printS s1 ^^ rightL ";") @@ printS s2
        | Stmt.If(c,t,f) -> 
            let _t = wordL "then" --- printS t
            let _f = wordL "else" --- printS f
            (wordL "if" ^^ bracketL(print c)) @@ _t @@ _f 
    
    let str ast = 
        printS ast
        |> Display.layout_to_string {FormatOptions.Default with PrintWidth = width}


module YCPP =
    open YC.PrettyPrinter
    open YC.PrettyPrinter.StructuredFormat

    let rec print ast =
        match ast with
        | Expr.BinOp(x, y, z) -> print y ++ wordL (string x) ^^ print z
        | Expr.Num n -> string n |> wordL
        | Expr.Var n -> string n |> wordL
    and printS ast =
        match ast with
        | Stmt.Write s -> wordL "write" ^^ bracketL(print s)
        | Stmt.Seq(s1, s2) -> (printS s1 >|< wordL ";") @@ printS s2
        | Stmt.If(c,t,f) -> 
            let _t = printS t
            let _f = printS f            
            let ifTpl f = (wordL "if" ^^ bracketL(print c)) @@ (wordL "then" |> f <| _t) @@ (wordL "else" |> f <| _f )            
            (ifTpl (^^)) >//< (ifTpl (@@--))

    let str ast = 
        printS ast
        |> Pretty.print width

do printfn "SF"
do SF.str ast |> printfn "%s"
do printfn "YCPP"
do YCPP.str ast |> printfn "%s"
