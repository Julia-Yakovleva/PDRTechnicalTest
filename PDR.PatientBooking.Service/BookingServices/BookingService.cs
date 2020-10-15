using Microsoft.EntityFrameworkCore;
using PDR.PatientBooking.Data;
using PDR.PatientBooking.Data.Models;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.BookingServices.Responses;
using PDR.PatientBooking.Service.BookingServices.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDR.PatientBooking.Service.BookingServices
{
    public class BookingService : IBookingService
    {
        private readonly PatientBookingContext _context;
        private readonly IAddBookingRequestValidator _validator;

        public BookingService(PatientBookingContext context, IAddBookingRequestValidator validator)
        {
            _context = context;
            _validator = validator;
        }

        public void AddBooking(AddBookingRequest request)
        {
            var validationResult = _validator.ValidateRequest(request);

            if (!validationResult.PassedValidation)
            {
                throw new ArgumentException(validationResult.Errors.First());
            }

            var surgeryType = _context.Patient
                .Include(p => p.Clinic)
                .Where(p => p.Id == request.PatientId)
                .Select(p => p.Clinic.SurgeryType)
                .FirstOrDefault();

            _context.Order.Add(new Order
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                SurgeryType = (int)surgeryType,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId
            });

            _context.SaveChanges();
        }

        public void CancelBooking(Guid bookingId)
        {
            var booking = _context.Order.FirstOrDefault(o => o.Id == bookingId);

            if (booking == null)
                throw new ArgumentException("Booking does not exist");

            booking.IsCancelled = true;

            _context.SaveChanges();
        }

        public GetPatientNextBookingResponse GetNextBooking(long patientId)
        {
            return _context.Order
                .Where(o => o.PatientId == patientId)
                .Where(o => o.StartTime > DateTime.Now)
                .OrderBy(o => o.StartTime)
                .Select(o => new GetPatientNextBookingResponse
                {
                    Id = o.Id,
                    DoctorId = o.DoctorId,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime
                })
                .FirstOrDefault();
        }
    }
}
