﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Localization;
using Volo.Abp.MultiLingualObject.TestObjects;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Testing;
using Xunit;

namespace Volo.Abp.MultiLingualObject
{
    public class MultiLingualObject_Tests : AbpIntegratedTest<AbpMultiLingualObjectTestModule>
    {
        private readonly IObjectMapper _objectMapper;
        private readonly IMultiLingualObjectManager _multiLingualObjectManager;
        private readonly MultiLingualBook _book;

        public MultiLingualObject_Tests()
        {
            _objectMapper = ServiceProvider.GetRequiredService<IObjectMapper>();
            _multiLingualObjectManager = ServiceProvider.GetRequiredService<IMultiLingualObjectManager>();

            var id = Guid.NewGuid();
            _book = new MultiLingualBook(id, 100)
            {
                Translations = new List<MultiLingualBookTranslation>()
            };

            var en = new MultiLingualBookTranslation
            {
                CoreId = id,
                Language = "en",
                Name = "C# in Depth",
                Core = _book
            };
            var zh = new MultiLingualBookTranslation
            {
                CoreId = id,
                Language = "zh-Hans",
                Name = "深入理解C#",
                Core = _book
            };

            _book.Translations.Add(en);
            _book.Translations.Add(zh);
        }

        protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options)
        {
            options.UseAutofac();
        }

        [Fact]
        public async Task Should_Map_Current_UI_Culture()
        {
            using (CultureHelper.Use("zh-Hans"))
            {
                var translation = await _multiLingualObjectManager.GetTranslationAsync<MultiLingualBook, MultiLingualBookTranslation>(_book);
                var bookDto = _objectMapper.Map<MultiLingualBookTranslation,MultiLingualBookDto>(translation);

                bookDto.Name.ShouldBe("深入理解C#");
                bookDto.Price.ShouldBe(_book.Price);
                bookDto.Id.ShouldBe(_book.Id);
            }
        }

        [Fact]
        public async Task Should_Map_Fallback_UI_Culture()
        {
            using (CultureHelper.Use("en-us"))
            {
                var translation = await _multiLingualObjectManager.GetTranslationAsync<MultiLingualBook, MultiLingualBookTranslation>(_book);
                var bookDto = _objectMapper.Map<MultiLingualBookTranslation,MultiLingualBookDto>(translation);

                bookDto.Name.ShouldBe("C# in Depth");
                bookDto.Price.ShouldBe(_book.Price);
                bookDto.Id.ShouldBe(_book.Id);
            }
        }

        [Fact]
        public async Task Should_Map_Default_Language()
        {
            using (CultureHelper.Use("tr"))
            {
                var translation = await _multiLingualObjectManager.GetTranslationAsync<MultiLingualBook, MultiLingualBookTranslation>(_book);
                var bookDto = _objectMapper.Map<MultiLingualBookTranslation,MultiLingualBookDto>(translation);

                bookDto.Name.ShouldBe("C# in Depth");
                bookDto.Price.ShouldBe(_book.Price);
                bookDto.Id.ShouldBe(_book.Id);
            }
        }

        [Fact]
        public async Task Should_Map_Specified_Language()
        {
            using (CultureHelper.Use("zh-Hans"))
            {
                var translation = await _multiLingualObjectManager.GetTranslationAsync<MultiLingualBook, MultiLingualBookTranslation>(_book, culture:"en");
                var bookDto = _objectMapper.Map<MultiLingualBookTranslation,MultiLingualBookDto>(translation);

                bookDto.Name.ShouldBe("C# in Depth");
                bookDto.Price.ShouldBe(_book.Price);
                bookDto.Id.ShouldBe(_book.Id);
            }
        }
    }

    public class MultiLingualBookObjectMapper : IObjectMapper<MultiLingualBookTranslation, MultiLingualBookDto>,
        ITransientDependency
    {

        public MultiLingualBookDto Map(MultiLingualBookTranslation source)
        {
            return new MultiLingualBookDto
            {
                Price = source.Core.Price,
                Id = source.CoreId,
                Name = source.Name
            };
        }

        public MultiLingualBookDto Map(MultiLingualBookTranslation source, MultiLingualBookDto destination)
        {
            return default;
        }
    }
}