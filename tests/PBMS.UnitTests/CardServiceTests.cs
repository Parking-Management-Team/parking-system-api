using NSubstitute;
using PBMS.Application.Card.DTOs;
using PBMS.Application.Card.Services;
using PBMS.Application.Contracts;
using PBMS.Domain.Entities;
using PBMS.Domain.Exceptions;

namespace PBMS.UnitTests
{
    public class CardServiceTests
    {
        private readonly ICardRepository _cardRepositoryMock;
        private readonly CardService _cardService;

        public CardServiceTests()
        {
            // 1. Arrange: Khởi tạo Mock Object cho ICardRepository
            _cardRepositoryMock = Substitute.For<ICardRepository>();
            
            // 2. Arrange: Inject Mock vào CardService
            _cardService = new CardService(_cardRepositoryMock);
        }

        // =========================================================================
        // UNIT TESTS CHO TÍNH NĂNG TẠO THẺ (Scenario 1)
        // =========================================================================

        [Fact]
        public async Task CreateCardAsync_ShouldCreateCard_WhenRfidCodeIsUnique()
        {
            // Arrange
            var request = new CreateCardRequest 
            { 
                RfidCode = "rfid-123", 
                CardType = "PARKING_CARD"
            };

            // Thiết lập Mock: Trả về false khi kiểm tra xem rfid-123 đã tồn tại chưa
            _cardRepositoryMock.IsRfidCodeExistsAsync("rfid-123").Returns(false);

            // Act
            var result = await _cardService.CreateCardAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("rfid-123", result.RfidCode);
            Assert.Equal("PARKING_CARD", result.CardType);
            Assert.Equal("Available", result.CardStatus); // Trạng thái mặc định

            // Kiểm tra xem repository có được gọi đúng các hàm lưu trữ hay không
            await _cardRepositoryMock.Received(1).AddAsync(Arg.Is<Card>(c => 
                c.RfidCode == "rfid-123" && 
                c.CardStatus == "Available"
            ));
            await _cardRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateCardAsync_ShouldThrowDomainException_WhenRfidCodeAlreadyExists()
        {
            // Arrange
            var request = new CreateCardRequest { RfidCode = "rfid-existing" };
            
            // Thiết lập Mock: Báo là RFID này đã tồn tại trong DB rồi
            _cardRepositoryMock.IsRfidCodeExistsAsync("rfid-existing").Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => 
                _cardService.CreateCardAsync(request)
            );

            Assert.Equal("RFID_CODE_EXISTS", exception.ErrorCode);
            
            // Đảm bảo không gọi hàm thêm mới và hàm lưu vào DB
            await _cardRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Card>());
            await _cardRepositoryMock.DidNotReceive().SaveChangesAsync();
        }

        // =========================================================================
        // UNIT TESTS CHO TÍNH NĂNG XÓA THẺ (Scenario 2)
        // =========================================================================

        [Fact]
        public async Task DeleteCardAsync_ShouldDeleteCard_WhenCardIsFree()
        {
            // Arrange
            int cardId = 99;
            var card = new Card { Id = cardId, RfidCode = "RFID-FREE" };

            // Thiết lập Mock: Tìm thấy thẻ và thẻ này KHÔNG bận đỗ xe
            _cardRepositoryMock.GetByIdAsync(cardId).Returns(card);
            _cardRepositoryMock.IsCardInActiveSessionAsync(cardId).Returns(false);

            // Act
            await _cardService.DeleteCardAsync(cardId);

            // Assert
            await _cardRepositoryMock.Received(1).RemoveAsync(card);
            await _cardRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task DeleteCardAsync_ShouldThrowDomainException_WhenCardIsUsedInActiveSession()
        {
            // Arrange
            int cardId = 100;
            var card = new Card { Id = cardId, RfidCode = "RFID-BUSY" };

            // Thiết lập Mock: Tìm thấy thẻ và thẻ này ĐANG bận đỗ xe
            _cardRepositoryMock.GetByIdAsync(cardId).Returns(card);
            _cardRepositoryMock.IsCardInActiveSessionAsync(cardId).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DomainException>(() => 
                _cardService.DeleteCardAsync(cardId)
            );

            Assert.Equal("CARD_IN_ACTIVE_SESSION", exception.ErrorCode);

            // Đảm bảo hàm Remove không được gọi
            await _cardRepositoryMock.DidNotReceive().RemoveAsync(Arg.Any<Card>());
            await _cardRepositoryMock.DidNotReceive().SaveChangesAsync();
        }
    }
}
