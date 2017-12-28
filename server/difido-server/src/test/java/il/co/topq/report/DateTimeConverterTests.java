package il.co.topq.report;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;

import org.junit.Before;
import org.junit.Test;
import static org.junit.Assert.assertEquals;

public class DateTimeConverterTests {

	private DateTimeConverter converter;

	@Before
	public void setup() {
		converter = new DateTimeConverter();
	}

	@Test
	public void testDateToElasticString() throws ParseException {
		String expectedDateString = "2017/12/28 20:38:32";
		Date date = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss").parse(expectedDateString);
		String actualDateString = converter.fromDateObject(date).toElasticTimestampString();
		assertEquals(expectedDateString, actualDateString);
	}
	
	@Test
	public void testElasticStingToDate() throws ParseException {
		String dateString = "2017/12/28 20:38:32";
		Date actDate = converter.fromElasticString(dateString).toDateObject();
		Date expDate = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss").parse(dateString);
		assertEquals(actDate.getTime(), expDate.getTime());
	}

	@Test
	public void testElasticStingToGmtDate() throws ParseException {
		Date actDate = converter.fromElasticString("2017/12/28 20:38:32").toGMTDateObject();
		Date expDate = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss").parse("2017/12/28 18:38:32");
		assertEquals(actDate.getTime(), expDate.getTime());
	}

}