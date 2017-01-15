using UnityEngine;
using System.Collections;
using Juniverse.RPCLibrary;
using Juniverse.Notifications;
using Juniverse.Model;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class DeviceNotificationManager : MonoBehaviour
{
    public delegate void SessionStarted(AttractionSessionStarted attractionSessionStarted);
    public event SessionStarted OnSessionStarted;
    public delegate void SessionEnded(AttractionSessionEnded attractionSessionEnded);
    public event SessionEnded OnSessionEnded;
    public delegate void SessionCanceled(AttractionSessionCanceled attractionSessionCanceled);
    public event SessionCanceled OnSessionCanceled;

    public delegate void PlayerSlotAdded(AttractionPlayerSlotAdded attractionPlayerSlotAdded);
    public event PlayerSlotAdded OnSlotAdded;

    public delegate void PlayerSlotRemoved(AttractionPlayerSlotRemoved attractionPlayerSlotRemoved);
    public event PlayerSlotRemoved OnPlayerSlotRemoved;

    public delegate void PlayerSlotCancelled(AttractionSlotCanceled attractionSlotCanceled);
    public event PlayerSlotCancelled OnSlotCancelled;

    public delegate void SlotPlayerAttendedd(AttractionSlotPlayerAttended attractionSlotPlayerAttended, QuestObjectiveData ObjectiveData, string PlayerAssignmentId);
    public event SlotPlayerAttendedd OnPlayerAttended;

    public delegate void PlayerCheckingWithoutReservation(AttractionPlayerCheckingWithoutReservation attractionPlayerCheckingWithoutReservation);
    public event PlayerCheckingWithoutReservation OnAttractionPlayerCheckingWithoutReservation;

    public delegate void AttractionDeviceStatusChanged(AttractionStatusChanged attractionStatusChanged);
    public event AttractionDeviceStatusChanged OnAttractionDeviceStatusChanged;

    AttractionStatus CurrentStatus = AttractionStatus.PlayersLeaving;

    public void BindNotifications()
    {
        DeviceManager.instance.MainGame.OnAttractionSessionStarted += AttractionSessionStartedNotification;
        DeviceManager.instance.MainGame.OnAttractionSessionCanceled += AttractionSessionCanceledNotification;
        DeviceManager.instance.MainGame.OnAttractionSessionEnded += AttractionSessionEndedNotification;
        DeviceManager.instance.MainGame.OnAttractionPlayerSlotAdded += AttractionPlayerSlotAddedNotification;
        DeviceManager.instance.MainGame.OnAttractionPlayerSlotRemoved += AttractionPlayerSlotRemovedNotification;
        DeviceManager.instance.MainGame.OnAttractionSlotPlayerAttended += AttractionSlotPlayerAttendedNotification;
        DeviceManager.instance.MainGame.OnAttractionPlayerCheckingWithoutReservation += AttractionPlayerCheckingWithoutReservationNotification;
        DeviceManager.instance.MainGame.OnAttractionSlotCanceled += AttractionSlotCanceledNotification;
        DeviceManager.instance.MainGame.OnAttractionStatusChanged += AttractionStatusChangedNotification;
    }

    void AttractionSessionEndedNotification(AttractionSessionEnded attractionSessionEnded)
    {
        print("AttractionSessionEndedNotification******************************************");
        if (OnSessionEnded != null)
            OnSessionEnded(attractionSessionEnded);
    }

    void AttractionPlayerSlotRemovedNotification(AttractionPlayerSlotRemoved attractionPlayerSlotRemoved)
    {
        if (OnPlayerSlotRemoved != null)
            OnPlayerSlotRemoved(attractionPlayerSlotRemoved);

        if (DeviceManager.instance != null && DeviceManager.instance.CurrentSession != null)
        {
            SubAttractionSession subsession;
            if (DeviceManager.instance.CurrentSession.SubSessions.TryGetValue(attractionPlayerSlotRemoved.SubSessionId, out subsession))
            {
                subsession.Slots.RemoveAll(x => x.ReservationId == attractionPlayerSlotRemoved.ReservationId);
                subsession.ShoppingSlots.RemoveAll(x => x.ReservationId == attractionPlayerSlotRemoved.ReservationId);
            }
        }

    }

    void AttractionStatusChangedNotification(AttractionStatusChanged attractionStatusChanged)
    {
        print("AttractionStatusChangedNotification");
        if (OnAttractionDeviceStatusChanged != null)
            OnAttractionDeviceStatusChanged(attractionStatusChanged);

        if (attractionStatusChanged.CurrentAttractionStatus == AttractionStatus.InSession && DeviceManager.instance.CurrentDeviceType != DeviceType.Terminal)
            JuniNetworkManager.Instance.StartGame();

        if (CurrentStatus == AttractionStatus.InSession)
        {
            bool bFinished = true;
            foreach (var item in ServerParameters.Instance.MyDoneObjectives)
            {
                if (!item)
                {
                    bFinished = false;
                    break;
                }
            }
            if (bFinished)
            {
                DeviceManager.instance.FinishObjective();
            }
        }
        CurrentStatus = attractionStatusChanged.CurrentAttractionStatus;
    }
    void AttractionSlotCanceledNotification(AttractionSlotCanceled attractionSlotCanceled)
    {
        if (OnSlotCancelled != null)
            OnSlotCancelled(attractionSlotCanceled);

        if (DeviceManager.instance != null && DeviceManager.instance.CurrentSession != null)
        {
            SubAttractionSession subsession;
            if (DeviceManager.instance.CurrentSession.SubSessions.TryGetValue(attractionSlotCanceled.SubSessionId, out subsession))
            {
                subsession.Slots.RemoveAll(x => x.ReservationId == attractionSlotCanceled.ReservationId);
                subsession.ShoppingSlots.RemoveAll(x => x.ReservationId == attractionSlotCanceled.ReservationId);
            }
        }
    }
    void AttractionPlayerCheckingWithoutReservationNotification(AttractionPlayerCheckingWithoutReservation attractionPlayerCheckingWithoutReservation)
    {
        DeviceManager.instance.MainGame.GetAttractionSessionAsync(DeviceManager.instance.AttractionId, attractionPlayerCheckingWithoutReservation.SessionId, (Session) =>
        {
            DeviceManager.instance.CurrentSession = Session;

            Juniverse.Model.SubAttractionSessionSlot slot = DeviceManager.instance.CurrentSession.SubSessions[DeviceManager.instance.SubAttractionID].Slots.Find(x => x.PlayerId == attractionPlayerCheckingWithoutReservation.UserId);
            if (DeviceManager.instance.CurrentDeviceType == DeviceType.Terminal && slot.AssignedTerminalId != DeviceManager.instance.TerminalSubattractionId)
                return;

            if (OnAttractionPlayerCheckingWithoutReservation != null)
                OnAttractionPlayerCheckingWithoutReservation(attractionPlayerCheckingWithoutReservation);

            if (OnPlayerAttended != null)
            {

                DeviceManager.instance.MainGame.GetCurrentPlayerObjectiveAsync(slot.PlayerId, DeviceManager.instance.SubAttractionID, slot.AssignmentId, (reply) =>
                {
                    OnPlayerAttended(new AttractionSlotPlayerAttended { PlayerId = attractionPlayerCheckingWithoutReservation.UserId, ReservationId = slot.ReservationId, SessionId = attractionPlayerCheckingWithoutReservation.SessionId, SubSessionId = DeviceManager.instance.SubAttractionID }, reply, slot.AssignmentId);
                }, (ex) => Debug.LogException(ex));
            }
        }, (ex) => Debug.LogException(ex));


    }
    void AttractionSessionCanceledNotification(AttractionSessionCanceled attractionSessionCanceled)
    {
        print("AttractionSessionCanceledNotification");
        if (OnSessionCanceled != null)
            OnSessionCanceled(attractionSessionCanceled);
    }
    void AttractionPlayerSlotAddedNotification(AttractionPlayerSlotAdded attractionPlayerSlotAdded)
    {
        if (OnSlotAdded != null)
            OnSlotAdded(attractionPlayerSlotAdded);
    }
    void AttractionSlotPlayerAttendedNotification(AttractionSlotPlayerAttended attractionSlotPlayerAttended)
    {
        Juniverse.Model.SubAttractionSessionSlot slot = DeviceManager.instance.CurrentSession.SubSessions[attractionSlotPlayerAttended.SubSessionId].Slots.Find(x => x.ReservationId == attractionSlotPlayerAttended.ReservationId);
        if (DeviceManager.instance.CurrentDeviceType == DeviceType.Terminal && slot.AssignedTerminalId != DeviceManager.instance.TerminalSubattractionId)
            return;

        if (OnPlayerAttended != null)
        {

            DeviceManager.instance.MainGame.GetCurrentPlayerObjectiveAsync(slot.PlayerId, DeviceManager.instance.SubAttractionID, slot.AssignmentId, (reply) =>
            {
                OnPlayerAttended(attractionSlotPlayerAttended, reply, slot.AssignmentId);
            }, (ex) => Debug.LogException(ex));
        }
    }

    void AttractionSessionStartedNotification(AttractionSessionStarted attractionSessionStarted)
    {
        if (attractionSessionStarted.AttractionId != DeviceManager.instance.AttractionId)
            return;

        print("AttractionSessionStartedNotification+++++++++++++++++++++++++++++++++++++++++++++++++");
        DeviceManager.instance.MainGame.GetAttractionSessionAsync(DeviceManager.instance.AttractionId, attractionSessionStarted.SessionId, (Session) =>
        {
            DeviceManager.instance.CurrentSession = Session;
            print("AttractionSessionStartedNotification---------------------------------------------");
            DeviceManager.instance.UpdateAttractionSessionStarted(attractionSessionStarted);
            if (OnSessionStarted != null)
                OnSessionStarted(attractionSessionStarted);
        }, (ex) => Debug.LogException(ex));
    }
}
