using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using System;

namespace PDR.PatientBooking.Service.BookingServices
{
    public interface IBookingService
    {
        void AddBooking(AddBookingRequest request);
        GetPatientNextBookingResponse GetNextBooking(long patientId);
        void CancelBooking(Guid bookingId);
    }
}
