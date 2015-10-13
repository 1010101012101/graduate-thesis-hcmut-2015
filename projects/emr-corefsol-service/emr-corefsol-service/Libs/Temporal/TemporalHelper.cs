﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace emr_corefsol_service.Libs
{
    using System.Text.RegularExpressions;
    using System.Web.Hosting;
    using Utilities;
    public class TemporalHelper
    {
        private PythonExcuter _excuter = null;
        private List<Regex> _inferred = null;
        private List<Regex> _explicit = null;
        private string year_re = @"[12][0-9][0-9][0-9]";
        private string month_re = @"[01][0-9]";
        private string day_re = @"[0123][0-9]";
        private string delimiter = @"[\\/-]";

        public TemporalHelper(PythonExcuter e)
        {
            _excuter = e;
            var normaliser = HostingEnvironment.MapPath(@"~\app_data\tools\Clinical-norma\normaliser.py");
            _excuter.SetScriptFile(normaliser);

            _inferred = new List<Regex>();
            _inferred.Add(new Regex(@"(?:the |her |his |their )?(?:post-|post|day)? ?(?:pod|operative|op|hospital|hsp|day|hd)(?:ly)? ?(?:day |night |afternoon )? ?(?:number|num\.?|#)? ?([0-9][0-9]*)"));
            _inferred.Add(new Regex(@"(?:the |her |his |their )?([0-9][0-9]*)(?:st|nd|rd|th)? (?:post-|post|day)? ?(?:pod|operative|op|hospital|hsp|day|hd)(?:ly)? (?:day|night|afternoon)?"));
            _inferred.Add(new Regex(@"(?:the)? ?([0-9][0-9]?)(?:st|nd|rd|th)? ?(-|to|or) ?([0-9][0-9]?)(?:st|nd|rd|th)? (?:post-|post|day)? ?(?:pod|operative|op|hospital|hsp|day|hd)(?:ly)? ?(?:days?|nights?|afternoons?)?"));
            _inferred.Add(new Regex(@"(?:the |her |his |their )?day (?:of )?(?:the )?(discharge|admission|transfer|evaluation)"));
            _inferred.Add(new Regex(@"(?:the|her|his|their)? ?day of life ?#? ?([0-9][0-9]*)"));
            _inferred.Add(new Regex(@"(?:early)? ?(?:post)? ?(?:-)? ?(operative|extubation) ?(?:course)"));

            _explicit = new List<Regex>();
            //yyyymmdd
            _explicit.Add(new Regex(year_re + month_re + day_re));
            //mm/dd/yyyy
            _explicit.Add(new Regex(month_re + delimiter +  day_re + delimiter + year_re));
            //yyyy/mm/dd
            _explicit.Add(new Regex(year_re + delimiter + month_re + delimiter + day_re));
            //dd/mm/yyyy
            _explicit.Add(new Regex(day_re + delimiter + month_re + delimiter + year_re));
            //dd/mm/yy
            _explicit.Add(new Regex(day_re + delimiter + month_re + delimiter + @"[0-9][0-9]"));
            //mm/dd/yy
            _explicit.Add(new Regex(month_re + delimiter + day_re + delimiter + @"[0-9][0-9]"));
        }

        public string GetTemporalValue(string emrPath, string line)
        {
            try
            {
                //Get explicit date
                var expl = GetExplicitDate(line);
                if(expl != null)
                {
                    return expl;
                }

                //Get inferred date
                return GetInferredDate(emrPath, line);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        private string GetExplicitDate(string line)
        {
            foreach(Regex r in _explicit)
            {
                var match = r.Match(line);
                if (match.Success)
                {
                    return match.Value;
                }
            }
            return null;
        }

        private string GetInferredDate(string emrPath, string line)
        {
            foreach(Regex r in _inferred)
            {
                var match = r.Match(line);
                if (match.Success)
                {
                    _excuter.Excute(new string[] { "normaliser.py", emrPath, match.Value });
                    dynamic result = _excuter.GetVariable("res");

                    if (result == null)
                    {
                        continue;
                    }

                    if(result[1] == "DATE")
                    {
                        return result[2];
                    }
                }
            }

            return null;
        }
    }
}