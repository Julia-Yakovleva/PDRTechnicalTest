using PDR.PatientBooking.Service.BookingServices.Requests;
using PDR.PatientBooking.Service.Validation;
using System;

namespace PDR.PatientBooking.Service.BookingServices.Validation
{
    public class AddBookingRequestValidator : IAddBookingRequestValidator
    {
        public PdrValidationResult ValidateRequest(AddBookingRequest request)
        {
            //validate for existing user
            //validate for existing doctor
            throw new NotImplementedException();
        }
    }
}
