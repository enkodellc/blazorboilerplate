using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSDCPortal.Theme.Material.Demo.Models
{
    public class AppointmentData
    {
        public int Id { get; set; }
        public string Subject { get; set; }   //Namw
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Nullable<bool> IsAllDay { get; set; }
        public string CategoryColor { get; set; }
        public string RecurrenceRule { get; set; }
        public Nullable<int> RecurrenceID { get; set; }
        public Nullable<int> FollowingID { get; set; }
        public string RecurrenceException { get; set; }
        public string StartTimezone { get; set; }
        public string EndTimezone { get; set; }
    }
    public class ContextEventsData : AppointmentData
    {
        public virtual Guid Guid { get; set; }
    }
    public class ReadonlyEventsData : AppointmentData
    {
        public bool IsReadonly { get; set; }
    }
    public class ResourceData : AppointmentData
    {
        public int ProjectId { get; set; }
        public int TaskId { get; set; }
    }
    public class BlockData : AppointmentData
    {
        public bool IsBlock { get; set; }
        public int EmployeeId { get; set; }
    }
    public class EmployeeData
    {
        public string Text { get; set; }
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string Color { get; set; }
        public string Designation { get; set; }
    }
    public class ResourceConferenceData : AppointmentData
    {
        public int[] ConferenceId { get; set; }
    }
    public class ConferenceData
    {
        public string Text { get; set; }
        public int Id { get; set; }
        public string Color { get; set; }
        public string Designation { get; set; }
    }
    public class RoomData : AppointmentData
    {
        public int RoomId { get; set; }
        public bool IsBlock { get; set; }
        public virtual string ElementType { get; set; }
        public virtual DateTime? StartTimeValue { get; set; }
        public virtual DateTime? EndTimeValue { get; set; }
    }
    public class RoomsData
    {
        public string Name { get; set; }
        public int? Id { get; set; }
        public int Capacity { get; set; }
        public string Color { get; set; }
        public string Type { get; set; }
    }
    public class ResourceTeamData : AppointmentData
    {
        public int ProjectId { get; set; }
        public int CategoryId { get; set; }
    }
    public class GroomingData : AppointmentData
    {
        public string Name { get; set; }
        public int DepartmentID { get; set; }
        public int ConsultantID { get; set; }
        public string DepartmentName { get; set; }
    }
    public class FifaEventsData : AppointmentData
    {
        public string City { get; set; }
        public int GroupId { get; set; }
    }
    public class DoctorsEventData : AppointmentData
    {
        public string EventType { get; set; }
    }
    public class WebinarData : AppointmentData
    {
        public string Tags { get; set; }
        public string ImageName { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
    }
    public class EventsData : AppointmentData
    {
        public string EventType { get; set; }
        public string City { get; set; }
    }
    public class ResourceEventsData : AppointmentData
    {
        public int CalendarId { get; set; }
    }
    public class DoctorData : AppointmentData
    {
        public int DoctorId { get; set; }
    }
    public class ResourceSampleData : AppointmentData
    {
        public int OwnerId { get; set; }
    }
}
