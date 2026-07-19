using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AutoMapper;
using NSubstitute;
using PBMS.Application.Common;
using PBMS.Application.Common.Exceptions;
using PBMS.Application.Contracts;
using PBMS.Application.Incident.DTOs;
using PBMS.Application.Incident.Interfaces;
using PBMS.Application.Incident.Services;
using PBMS.Domain.Entities;
using PBMS.Domain.Enums;
using Xunit;

namespace PBMS.UnitTests
{
    public class IncidentServiceTests
    {
        private readonly IIncidentRepository _incidentRepositoryMock;
        private readonly IRepository<IncidentType> _incidentTypeRepositoryMock;
        private readonly IRepository<PBMS.Domain.Entities.ParkingSession> _sessionRepositoryMock;
        private readonly IMapper _mapperMock;
        private readonly IFeeCalculatorService _feeCalculatorServiceMock;
        private readonly IPenaltyConfigRepository _penaltyConfigRepositoryMock;
        private readonly ICardRepository _cardRepositoryMock;
        private readonly IncidentService _incidentService;

        public IncidentServiceTests()
        {
            _incidentRepositoryMock = Substitute.For<IIncidentRepository>();
            _incidentTypeRepositoryMock = Substitute.For<IRepository<IncidentType>>();
            _sessionRepositoryMock = Substitute.For<IRepository<PBMS.Domain.Entities.ParkingSession>>();
            _mapperMock = Substitute.For<IMapper>();
            _feeCalculatorServiceMock = Substitute.For<IFeeCalculatorService>();
            _penaltyConfigRepositoryMock = Substitute.For<IPenaltyConfigRepository>();
            _cardRepositoryMock = Substitute.For<ICardRepository>();

            _incidentService = new IncidentService(
                _incidentRepositoryMock,
                _incidentTypeRepositoryMock,
                _sessionRepositoryMock,
                _mapperMock,
                _feeCalculatorServiceMock,
                _penaltyConfigRepositoryMock,
                _cardRepositoryMock
            );
        }

        [Fact]
        public async Task ReportIncidentAsync_ShouldFail_WhenPenaltyFeeIsNegative()
        {
            // Arrange
            var request = new ReportIncidentRequest
            {
                SessionId = 1,
                IncidentTypeId = 2,
                PenaltyFee = -5000 // Tiền phạt âm
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => 
                _incidentService.ReportIncidentAsync(request));
            Assert.Equal("Penalty fee cannot be negative.", exception.Message);
        }

        [Fact]
        public async Task ReportIncidentAsync_ShouldFail_WhenSessionIsCompleted()
        {
            // Arrange
            var request = new ReportIncidentRequest
            {
                SessionId = 1,
                IncidentTypeId = 2
            };

            var session = new PBMS.Domain.Entities.ParkingSession
            {
                Id = 1,
                SessionStatus = "COMPLETED" // Phiên đã hoàn tất
            };

            _sessionRepositoryMock.GetByIdAsync(1).Returns(session);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => 
                _incidentService.ReportIncidentAsync(request));
            Assert.Equal("Cannot report an incident on a completed parking session.", exception.Message);
        }

        [Fact]
        public async Task ReportIncidentAsync_ShouldFail_WhenDuplicateIncidentExists()
        {
            // Arrange
            var request = new ReportIncidentRequest
            {
                SessionId = 1,
                IncidentTypeId = 2
            };

            var session = new PBMS.Domain.Entities.ParkingSession
            {
                Id = 1,
                SessionStatus = "ACTIVE"
            };

            var incidentType = new IncidentType
            {
                Id = 2,
                IncidentCode = "LOST_CARD",
                IncidentName = "Lost Card"
            };

            var existingIncident = new PBMS.Domain.Entities.Incident
            {
                Id = 10,
                SessionId = 1,
                IncidentTypeId = 2,
                Status = IncidentStatus.Open
            };

            _sessionRepositoryMock.GetByIdAsync(1).Returns(session);
            _incidentTypeRepositoryMock.GetByIdAsync(2).Returns(incidentType);
            _incidentRepositoryMock.FirstOrDefaultAsync(Arg.Any<Expression<Func<PBMS.Domain.Entities.Incident, bool>>>())
                .Returns(existingIncident); // Đã tồn tại sự cố Open cùng loại

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(() => 
                _incidentService.ReportIncidentAsync(request));
            Assert.Contains("already exists for this session", exception.Message);
        }

        [Fact]
        public async Task ReportIncidentAsync_ShouldMarkCardAsLost_WhenReportingLostCard()
        {
            // Arrange
            var request = new ReportIncidentRequest
            {
                SessionId = 1,
                IncidentTypeId = 2
            };

            var session = new PBMS.Domain.Entities.ParkingSession
            {
                Id = 1,
                CardId = 55,
                SessionStatus = "ACTIVE"
            };

            var incidentType = new IncidentType
            {
                Id = 2,
                IncidentCode = "LOST_CARD",
                IncidentName = "Lost Card"
            };

            var card = new Card
            {
                Id = 55,
                CardStatus = CardStatus.Active.ToString()
            };

            _sessionRepositoryMock.GetByIdAsync(1).Returns(session);
            _incidentTypeRepositoryMock.GetByIdAsync(2).Returns(incidentType);
            _cardRepositoryMock.GetByIdAsync(55).Returns(card);

            // Act
            await _incidentService.ReportIncidentAsync(request);

            // Assert
            Assert.Equal(CardStatus.Lost.ToString(), card.CardStatus); // Phải chuyển sang Lost
            _cardRepositoryMock.Received(1).Update(card);
        }

        [Fact]
        public async Task UpdateIncidentStatusAsync_ShouldBlockCard_WhenResolvingLostCardIncident()
        {
            // Arrange
            int incidentId = 10;
            var request = new UpdateIncidentStatusRequest
            {
                Status = IncidentStatus.Resolved
            };

            var incident = new PBMS.Domain.Entities.Incident
            {
                Id = incidentId,
                SessionId = 1,
                IncidentTypeId = 2,
                Status = IncidentStatus.Open
            };

            var incidentType = new IncidentType
            {
                Id = 2,
                IncidentCode = "LOST_CARD"
            };

            var session = new PBMS.Domain.Entities.ParkingSession
            {
                Id = 1,
                CardId = 55
            };

            var card = new Card
            {
                Id = 55,
                CardStatus = CardStatus.Lost.ToString()
            };

            _incidentRepositoryMock.GetByIdAsync(incidentId).Returns(incident);
            _incidentTypeRepositoryMock.GetByIdAsync(2).Returns(incidentType);
            _sessionRepositoryMock.GetByIdAsync(1).Returns(session);
            _cardRepositoryMock.GetByIdAsync(55).Returns(card);

            // Act
            await _incidentService.UpdateIncidentStatusAsync(incidentId, request);

            // Assert
            Assert.Equal(CardStatus.Blocked.ToString(), card.CardStatus); // Phải khóa vĩnh viễn (Blocked)
            _cardRepositoryMock.Received(1).Update(card);
        }
    }
}
