﻿using MilSpace.Configurations;
using MilSpace.DataAccess.DataTransfer;
using MilSpace.DataAccess.Definition;
using MilSpace.DataAccess.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MilSpace.DataAccess.Facade
{
    internal class SemanticDataAccess : DataAccessor<MilSpaceStorageContext>, IDisposable
    {
        public override string ConnectionString => MilSpaceConfiguration.ConnectionProperty.WorkingDBConnection;


        public bool SaveUserSession(int profileIdId, bool submit = false)
        {
            bool result = true;
            try
            {
                string userName = Environment.UserName;

                if (context.MilSp_Profiles.Any(p => p.idRow == profileIdId) || !submit)
                {
                    if (!context.MilSp_Sessions.Any(s => s.userName == userName && s.ProfileId == profileIdId))
                    {
                        var userSession = new MilSp_Session()
                        {
                            ProfileId = profileIdId,
                            userName = userName
                        };

                        context.MilSp_Sessions.InsertOnSubmit(userSession);

                        if (submit)
                        {
                            Submit();
                        }
                    }
                }
                else
                {
                    throw new MilSpaceProfileNotFoundException(profileIdId);
                }

            }
            catch (MilSpaceDataException ex)
            {
                log.WarnEx(ex.Message);
                if (ex.InnerException != null)
                {
                    log.WarnEx(ex.InnerException.Message);
                }

                result = false;
            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
                result = false;
            }
            return result;
        }

        public bool CanEraseProfileSession(int profileIdId)
        {
            bool result = false;

            try
            {
                string userName = Environment.UserName;

                var profile = context.MilSp_Profiles.FirstOrDefault(p => p.idRow == profileIdId);

                if (profile != null)
                {
                    result = !profile.MilSp_Sessions.Any(s => s.userName != userName);
                }
            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
                result = false;
            }

            return result;
        }

        public bool EraseProfileSession(int profileIdId)
        {
            bool result = true;

            try
            {
                string userName = Environment.UserName;
                var userSessions = context.MilSp_Sessions.Where(s => s.ProfileId == profileIdId);
                if (userSessions.Any())
                {
                    userSessions.ToList().ForEach(s => context.MilSp_Sessions.DeleteOnSubmit(s));
                }

                var profileSession = context.MilSp_Profiles.FirstOrDefault(p => p.idRow == profileIdId);
                if (profileSession != null)
                {
                    context.MilSp_Profiles.DeleteOnSubmit(profileSession);
                }

                Submit();

            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
                result = false;
            }


            return result;
        }

        public bool DeleteUserSession(int profileIdId)
        {
            bool result = true;
            try
            {
                string userName = Environment.UserName;
                if (context.MilSp_Sessions.Any(s => s.userName == userName && s.ProfileId == profileIdId))
                {
                    var sessionRow = context.MilSp_Sessions.First(s => s.userName == userName && s.ProfileId == profileIdId);
                    context.MilSp_Sessions.DeleteOnSubmit(sessionRow);

                    Submit();
                }

            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
                result = false;
            }
            return result;
        }

        public bool SaveProfileSession(ProfileSession session)
        {
            bool result = true;
            try
            {
                var profile = context.MilSp_Profiles.FirstOrDefault(p => p.idRow == session.SessionId);
                if (profile != null)
                {
                    profile.ProfileData = session.Serialized;
                    profile.Shared = session.Shared;
                }
                else
                {
                    profile = session.Get();
                    context.MilSp_Profiles.InsertOnSubmit(profile);
                }


                if (SaveUserSession(profile.idRow))
                {
                    Submit();
                }
            }
            catch (MilSpaceDataException ex)
            {
                log.WarnEx(ex.Message);
                if (ex.InnerException != null)
                {
                    log.WarnEx(ex.InnerException.Message);
                }

                result = false;
            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
                result = false;
            }
            return result;
        }

        public ProfileSession GetProfileSessionById(int sessionId)
        {
            try
            {
                var session = context.MilSp_Profiles.FirstOrDefault(s => s.idRow == sessionId);
                if (session != null)
                {
                    return session.Get();
                }

            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
            }

            return null;
        }

        public IEnumerable<ProfileSession> GetAllSessionsForUser()
        {
            try
            {
                var result = context.MilSp_Sessions.Where(s => s.userName == Environment.UserName).Select(s => s.MilSp_Profile.Get());
                log.InfoEx($"Get all profiles ({result.Count()}) for user {Environment.UserName}");
                return result;
            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
            }

            return null;
        }


        public IEnumerable<ProfileSession> GetAvailableProfiles()
        {
            try
            {
                var result = context.MilSp_Profiles.Where(s => s.Creator == Environment.UserName || s.Shared).Select(s => s.Get());
                log.InfoEx($"Get all profiles ({result.Count()}) for user {Environment.UserName} or shared with him");
                return result;
            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
            }

            return null;
        }

        #region Visibility
        public IEnumerable<ObservationPoint> GetObservationPoints()
        {
            try
            {
                var result = context.VisiblilityObservPoints.Select(op => op.Get());
                log.InfoEx($"Get all Observation point ({result.Count()}). user {Environment.UserName}");
                return result;
            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
            }
            return null;
        }

        public bool SaveObservationPoint(ObservationPoint observPoint)
        {
            bool result = true;
            try
            {
                var bdObservPoint = context.VisiblilityObservPoints.FirstOrDefault(p => p.OBJECTID == observPoint.Objectid);
                if (bdObservPoint != null)
                {
                    bdObservPoint.Update(observPoint);
                    if (Submit())
                    {
                        log.InfoEx($"Observation Point Row with ObjectId '{observPoint.Objectid}' was saved");
                    }
                }
            }
            catch (MilSpaceDataException ex)
            {
                log.WarnEx(ex.Message);
                if (ex.InnerException != null)
                {
                    log.WarnEx(ex.InnerException.Message);
                }

                result = false;
            }
            catch (Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
                result = false;
            }
            return result;

        }

       

    }

    internal class VisibilityDataAccess : DataAccessor<MilSpaceVisibilityContext>, IDisposable
    {
        public override string ConnectionString => MilSpaceConfiguration.ConnectionProperty.WorkingDBConnection;

        public bool SaveVisibilitySession(VisibilitySession visibilitySession)
        {
            try
            {
               if (!context.MilSp_VisibilitySessions.Any(session => session.Id == visibilitySession.Id))
               {
                    var  sessionEntity = visibilitySession.Get();
                    context.MilSp_VisibilitySessions.InsertOnSubmit(sessionEntity);

                    Submit();
                    return true;
               }

                log.WarnEx($"Session with the same id already exists");
            }
            catch(MilSpaceDataException ex)
            {
                log.WarnEx(ex.Message);

                if(ex.InnerException != null)
                {
                    log.WarnEx(ex.InnerException.Message);
                }
            }
            catch(Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
            }

            return false;
        }

        public bool UpdateVisibilitySession(VisibilitySession visibilitySession)
        {
            try
            {
                var sessionEntity = context.MilSp_VisibilitySessions.FirstOrDefault(session => session.Id == visibilitySession.Id);
                
                if(sessionEntity != null)
                {
                   sessionEntity.Update(visibilitySession);
                   
                    Submit();
                    return true;
                }

                log.WarnEx($"Session not found");
            }
            catch(MilSpaceDataException ex)
            {
                log.WarnEx(ex.Message);

                if(ex.InnerException != null)
                {
                    log.WarnEx(ex.InnerException.Message);
                }
            }
            catch(Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
            }

            return false;
        }

        public bool DeleteVisibilitySession(string id)
        {
            try
            {
                var sessionEntity = context.MilSp_VisibilitySessions.FirstOrDefault(session => session.Id == id);

                if(sessionEntity != null)
                {
                    context.MilSp_VisibilitySessions.DeleteOnSubmit(sessionEntity);

                    Submit();
                    return true;
                }

                log.WarnEx($"Session not found");
            }
            catch(Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
            }

            return false;
        }

        public IEnumerable<VisibilitySession> GetAllVisibilitySessions()
        {
            try
            {
                var sessions = context.MilSp_VisibilitySessions.Select(s => s.Get());
                return sessions;
            }
            catch(Exception ex)
            {
                log.WarnEx($"Unexpected exception:{ex.Message}");
            }

            return null;
        }
    }
    #endregion
}
