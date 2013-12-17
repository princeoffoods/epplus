﻿/*******************************************************************************
 * You may amend and distribute as you like, but don't remove this header!
 *
 * EPPlus provides server-side generation of Excel 2007/2010 spreadsheets.
 * See http://www.codeplex.com/EPPlus for details.
 *
 * Copyright (C) 2011  Jan Källman
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.

 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  
 * See the GNU Lesser General Public License for more details.
 *
 * The GNU Lesser General Public License can be viewed at http://www.opensource.org/licenses/lgpl-license.php
 * If you unfamiliar with this license or have questions about it, here is an http://www.gnu.org/licenses/gpl-faq.html
 *
 * All code and executables are provided "as is" with no warranty either express or implied. 
 * The author accepts no liability for any damage or loss of business that this product may cause.
 *
 * Code change notes:
 * 
 * Author							Change						Date
 * ******************************************************************************
 * Jan Källman                      Added                       2012-03-04  
 *******************************************************************************/
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using OfficeOpenXml.Calculation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml.FormulaParsing;
namespace OfficeOpenXml.Calculation
{
    public static class CalculationExtension
    {
        public static void Calculate(this ExcelWorkbook workbook)
        {
            Init(workbook);

            var dc = DependencyChainFactory.Create(workbook);
            var parser = workbook.FormulaParser;
            //TODO: Add calculation here
            foreach (var ix in dc.CalcOrder)
            {
                var item = dc.list[ix];
                try
                {
                    var v = parser.ParseCell(item.Tokens, item.ws == null ? "" : item.ws.Name, item.Row, item.Column);
                    SetValue(workbook, item, v);
                }
                catch (Exception e)
                {
                    // TODO: add errorhandling here...
                }
                
            }            
            workbook._isCalculated = true;
        }
        public static void Calculate(this ExcelWorksheet worksheet)
        {
            Init(worksheet.Workbook);
            var parser = worksheet.Workbook.FormulaParser;
            var dc = DependencyChainFactory.Create(worksheet);
            foreach (var ix in dc.CalcOrder)
            {
                var item = dc.list[ix];
                var v = parser.ParseCell(item.Tokens, item.ws.Name, item.Row, item.Column);
                SetValue(worksheet.Workbook, item, v);
            }
            worksheet.Workbook._isCalculated = true;
        }
        public static void Calculate(this ExcelRangeBase range)
        {
            Init(range._workbook);
            var parser = range._workbook.FormulaParser;
            var dc = DependencyChainFactory.Create(range);
            foreach (var ix in dc.CalcOrder)
            {
                var item = dc.list[ix];
                var v = parser.ParseCell(item.Tokens, item.ws.Name, item.Row, item.Column);
                SetValue(range._workbook, item, v);
            }
            range.Worksheet.Workbook._isCalculated = true;
        }
        private static void Init(ExcelWorkbook workbook)
        {
            workbook._formulaTokens = new CellStore<List<Token>>();;
            foreach (var ws in workbook.Worksheets)
            {
                if (ws._formulaTokens != null)
                {
                    ws._formulaTokens.Dispose();
                }
                ws._formulaTokens = new CellStore<List<Token>>();
            }
        }

        private static void SetValue(ExcelWorkbook workbook, FormulaCell item, object v)
        {
            if (item.Column == 0)
            {
                if (item.SheetID == 0)
                {
                    workbook.Names[item.Row].NameValue = v;
                }
                else
                {
                    var sh = workbook.Worksheets.GetBySheetID(item.SheetID);
                    sh.Names[item.Index].NameValue = v;
                }
            }
            else
            {
                var sheet = workbook.Worksheets.GetBySheetID(item.SheetID);
                sheet._values.SetValue(item.Row, item.Column, v);
            }
        }
    }
}