using PDR.PatientBooking.Data;
using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        private readonly PatientBookingContext _context;

        public AddBookingRequestValidator(PatientBookingContext context)
        {
            _context = context;
        }

        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            var result = new PdrValidationResult(true);

            if (InvalidFields(request, ref result))
                return result;

            if (DoctorNotFound(request, ref result))
                return result;

            if (PatientNotFound(request, ref result))
                return result;

            if (DoctorIsBusy(request, ref result))
                return result;

            return result;
        }

        public bool InvalidFields(AddBookingRequest request, ref PdrValidationResult result)
        {
            var errors = new List<string>();

            // Using UtcNow for the sake of simplicity.
            // In real application time zones should be considered.
            // One of the approaches is to store everything in UTC time and compare converted client time.
            // Another approach is to store client's time and timezone.
            if (request.StartTime <= DateTime.UtcNow)
                errors.Add("Appointment should start in the future");

            if (request.StartTime >= request.EndTime)
                errors.Add("Start time should be prior to end time");

            if (errors.Any())
            {
                result.PassedValidation = false;
                result.Errors.AddRange(errors);
                return true;
            }

            return false;
        }

        private bool DoctorIsBusy(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (_context.Order.Any(o => 
                o.DoctorId == request.DoctorId &&
                (request.StartTime > o.StartTime && request.StartTime < o.EndTime ||
                request.EndTime > o.StartTime && request.EndTime < o.EndTime ||
                request.StartTime <= o.StartTime && request.EndTime >= o.EndTime)))
            {
                result.PassedValidation = false;
                result.Errors.Add("The doctor is busy");
                return true;
            }

            return false;
        }

        private bool DoctorNotFound(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (! _context.Doctor.Any(d => d.Id == request.DoctorId))
            {
                result.PassedValidation = false;
                result.Errors.Add("The doctor not found");
                return true;
            }

            return false;
        }

        private bool PatientNotFound(AddBookingRequest request, ref PdrValidationResult result)
        {
            if (! _context.Patient.Any(p => p.Id == request.PatientId))
            {
                result.PassedValidation = false;
                result.Errors.Add("The patient not found");
                return true;
            }

            return false;
        }
    }
}
